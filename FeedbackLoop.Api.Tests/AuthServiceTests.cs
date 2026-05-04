using FeedbackLoop.Api.Application.Services;
using FeedbackLoop.Api.Domain.DTOs.Auth;
using FeedbackLoop.Api.Domain.Entities;
using FeedbackLoop.Api.Domain.Enums;
using FeedbackLoop.Api.Domain.Exceptions;
using FeedbackLoop.Api.Infrastructure.Auth;
using FeedbackLoop.Api.Infrastructure.Time;
using FeedbackLoop.Api.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace FeedbackLoop.Api.Tests;

public sealed class AuthServiceTests
{
    private static readonly DateTime Now = new(2026, 5, 3, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Register_WithValidData_ReturnsAuthResponse()
    {
        var fixture = new AuthServiceFixture();
        User? createdUser = null;
        Workspace? createdWorkspace = null;
        RefreshToken? createdRefreshToken = null;

        fixture.Users
            .Setup(repository => repository.EmailExistsAsync("admin@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        fixture.Users
            .Setup(repository => repository.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => createdUser = user)
            .Returns(Task.CompletedTask);

        fixture.Workspaces
            .Setup(repository => repository.AddAsync(It.IsAny<Workspace>(), It.IsAny<CancellationToken>()))
            .Callback<Workspace, CancellationToken>((workspace, _) => createdWorkspace = workspace)
            .Returns(Task.CompletedTask);

        fixture.RefreshTokens
            .Setup(repository => repository.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Callback<RefreshToken, CancellationToken>((token, _) => createdRefreshToken = token)
            .Returns(Task.CompletedTask);

        var response = await fixture.Service.RegisterAsync(
            new RegisterRequest("Admin User", "Admin@Example.com", "StrongPassword123!", "Acme Inc"),
            "browser");

        response.AccessToken.Should().NotBeNullOrWhiteSpace();
        response.RefreshToken.Should().NotBeNullOrWhiteSpace();
        response.ExpiresAt.Should().Be(Now.AddMinutes(15));
        response.User.Email.Should().Be("admin@example.com");
        response.User.Role.Should().Be(UserRole.Admin);
        createdWorkspace.Should().NotBeNull();
        createdWorkspace!.Slug.Should().Be("acme-inc");
        createdUser.Should().NotBeNull();
        createdUser!.WorkspaceId.Should().Be(createdWorkspace.Id);
        createdRefreshToken.Should().NotBeNull();
        createdRefreshToken!.Token.Should().NotBe(response.RefreshToken);
        createdRefreshToken.Token.Should().Be(AuthService.HashToken(response.RefreshToken));
        fixture.UnitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ThrowsConflictException()
    {
        var fixture = new AuthServiceFixture();
        fixture.Users
            .Setup(repository => repository.EmailExistsAsync("admin@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = () => fixture.Service.RegisterAsync(
            new RegisterRequest("Admin User", "admin@example.com", "StrongPassword123!", "Acme Inc"));

        await act.Should().ThrowAsync<ConflictException>();
        fixture.UnitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ThrowsUnauthorizedException()
    {
        var fixture = new AuthServiceFixture();
        var user = CreateUser(password: "CorrectPassword123!");

        fixture.Users
            .Setup(repository => repository.GetByEmailAsync("admin@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var act = () => fixture.Service.LoginAsync(new LoginRequest("admin@example.com", "WrongPassword123!"));

        await act.Should().ThrowAsync<UnauthorizedDomainException>();
        fixture.UnitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Refresh_WithExpiredToken_ThrowsUnauthorizedException()
    {
        var fixture = new AuthServiceFixture();
        var rawToken = "expired-token";
        var storedToken = CreateRefreshToken(rawToken, CreateUser(), expiresAt: Now.AddMinutes(-1));

        fixture.RefreshTokens
            .Setup(repository => repository.GetByTokenHashAsync(AuthService.HashToken(rawToken), It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        var act = () => fixture.Service.RefreshAsync(new RefreshRequest(rawToken));

        await act.Should().ThrowAsync<UnauthorizedDomainException>();
        fixture.UnitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Refresh_WithRevokedToken_ThrowsUnauthorizedException()
    {
        var fixture = new AuthServiceFixture();
        var rawToken = "revoked-token";
        var storedToken = CreateRefreshToken(rawToken, CreateUser(), expiresAt: Now.AddDays(1));
        storedToken.RevokedAt = Now.AddMinutes(-5);

        fixture.RefreshTokens
            .Setup(repository => repository.GetByTokenHashAsync(AuthService.HashToken(rawToken), It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        var act = () => fixture.Service.RefreshAsync(new RefreshRequest(rawToken));

        await act.Should().ThrowAsync<UnauthorizedDomainException>();
        fixture.RefreshTokens.Verify(
            repository => repository.RevokeTokenFamilyAsync(storedToken.UserId, storedToken.FamilyId, Now, It.IsAny<CancellationToken>()),
            Times.Once);
        fixture.UnitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Refresh_ValidToken_RevokesOldAndReturnsNew()
    {
        var fixture = new AuthServiceFixture();
        var rawToken = "valid-token";
        var user = CreateUser();
        var storedToken = CreateRefreshToken(rawToken, user, expiresAt: Now.AddDays(1));
        RefreshToken? newRefreshToken = null;

        fixture.RefreshTokens
            .Setup(repository => repository.GetByTokenHashAsync(AuthService.HashToken(rawToken), It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        fixture.RefreshTokens
            .Setup(repository => repository.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Callback<RefreshToken, CancellationToken>((token, _) => newRefreshToken = token)
            .Returns(Task.CompletedTask);

        var response = await fixture.Service.RefreshAsync(new RefreshRequest(rawToken), "browser");

        storedToken.RevokedAt.Should().Be(Now);
        response.RefreshToken.Should().NotBeNullOrWhiteSpace();
        response.RefreshToken.Should().NotBe(rawToken);
        newRefreshToken.Should().NotBeNull();
        newRefreshToken!.FamilyId.Should().Be(storedToken.FamilyId);
        newRefreshToken.Token.Should().Be(AuthService.HashToken(response.RefreshToken));
        fixture.UnitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static User CreateUser(string password = "StrongPassword123!")
    {
        var workspaceId = Guid.NewGuid();
        return new User
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            Workspace = new Workspace { Id = workspaceId, Name = "Acme", Slug = "acme" },
            Email = "admin@example.com",
            DisplayName = "Admin User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = UserRole.Admin,
            IsActive = true
        };
    }

    private static RefreshToken CreateRefreshToken(string rawToken, User user, DateTime expiresAt)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = AuthService.HashToken(rawToken),
            WorkspaceId = user.WorkspaceId,
            UserId = user.Id,
            User = user,
            FamilyId = Guid.NewGuid(),
            CreatedAt = Now.AddMinutes(-10),
            ExpiresAt = expiresAt
        };
    }

    private sealed class AuthServiceFixture
    {
        public AuthServiceFixture()
        {
            Service = new AuthService(
                Users.Object,
                Workspaces.Object,
                RefreshTokens.Object,
                UnitOfWork.Object,
                Options.Create(new JwtSettings
                {
                    Secret = "test-secret-with-enough-length-for-hmac-sha256",
                    Issuer = "feedbackloop-api",
                    Audience = "feedbackloop-clients",
                    AccessTokenExpirationMinutes = 15,
                    RefreshTokenExpirationDays = 7
                }),
                new FixedClock(Now));
        }

        public Mock<IUserRepository> Users { get; } = new();

        public Mock<IWorkspaceRepository> Workspaces { get; } = new();

        public Mock<IRefreshTokenRepository> RefreshTokens { get; } = new();

        public Mock<IUnitOfWork> UnitOfWork { get; } = new();

        public AuthService Service { get; }
    }

    private sealed class FixedClock : ISystemClock
    {
        public FixedClock(DateTime utcNow)
        {
            UtcNow = utcNow;
        }

        public DateTime UtcNow { get; }
    }
}
