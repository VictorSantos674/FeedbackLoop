using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FeedbackLoop.Api.Domain.DTOs.Auth;
using FeedbackLoop.Api.Domain.Entities;
using FeedbackLoop.Api.Domain.Enums;
using FeedbackLoop.Api.Domain.Exceptions;
using FeedbackLoop.Api.Infrastructure.Auth;
using FeedbackLoop.Api.Infrastructure.Time;
using FeedbackLoop.Api.Repositories;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FeedbackLoop.Api.Application.Services;

public sealed class AuthService : IAuthService
{
    private const string InvalidCredentialsMessage = "Invalid credentials.";

    private readonly IUserRepository _users;
    private readonly IWorkspaceRepository _workspaces;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;
    private readonly ISystemClock _clock;

    public AuthService(
        IUserRepository users,
        IWorkspaceRepository workspaces,
        IRefreshTokenRepository refreshTokens,
        IUnitOfWork unitOfWork,
        IOptions<JwtSettings> jwtOptions,
        ISystemClock clock)
    {
        _users = users;
        _workspaces = workspaces;
        _refreshTokens = refreshTokens;
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtOptions.Value;
        _clock = clock;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, string? userAgent = null, CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        if (await _users.EmailExistsAsync(email, cancellationToken))
        {
            throw new ConflictException("Email already registered.");
        }

        var workspace = new Workspace
        {
            Name = request.WorkspaceName.Trim(),
            Slug = Slugify(request.WorkspaceName),
            CreatedAtUtc = _clock.UtcNow
        };

        var user = new User
        {
            Workspace = workspace,
            WorkspaceId = workspace.Id,
            DisplayName = request.Name.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.Admin,
            CreatedAtUtc = _clock.UtcNow
        };

        await _workspaces.AddAsync(workspace, cancellationToken);
        await _users.AddAsync(user, cancellationToken);

        var response = await CreateAuthResponseAsync(user, userAgent, familyId: null, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, string? userAgent = null, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByEmailAsync(NormalizeEmail(request.Email), cancellationToken);
        if (user is null || !user.IsActive || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedDomainException(InvalidCredentialsMessage);
        }

        var now = _clock.UtcNow;
        await _refreshTokens.RevokeActiveTokensForDeviceAsync(user.Id, HashUserAgent(userAgent), now, cancellationToken);

        var response = await CreateAuthResponseAsync(user, userAgent, familyId: null, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }

    public async Task<AuthResponse> RefreshAsync(RefreshRequest request, string? userAgent = null, CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(request.RefreshToken);
        var storedToken = await _refreshTokens.GetByTokenHashAsync(tokenHash, cancellationToken);
        var now = _clock.UtcNow;

        if (storedToken is null || storedToken.ExpiresAt <= now || storedToken.User is null || !storedToken.User.IsActive)
        {
            throw new UnauthorizedDomainException(InvalidCredentialsMessage);
        }

        if (storedToken.RevokedAt.HasValue)
        {
            await _refreshTokens.RevokeTokenFamilyAsync(storedToken.UserId, storedToken.FamilyId, now, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedDomainException(InvalidCredentialsMessage);
        }

        storedToken.RevokedAt = now;

        var response = await CreateAuthResponseAsync(storedToken.User, userAgent, storedToken.FamilyId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }

    public async Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(request.RefreshToken);
        var storedToken = await _refreshTokens.GetByTokenHashAsync(tokenHash, cancellationToken);
        if (storedToken is null)
        {
            return;
        }

        if (!storedToken.RevokedAt.HasValue)
        {
            storedToken.RevokedAt = _clock.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<AuthResponse> CreateAuthResponseAsync(User user, string? userAgent, Guid? familyId, CancellationToken cancellationToken)
    {
        var accessTokenExpiresAt = _clock.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);
        var accessToken = GenerateAccessToken(user, accessTokenExpiresAt);
        var refreshToken = await GenerateRefreshTokenAsync(user, userAgent, familyId, cancellationToken);

        return new AuthResponse(
            accessToken,
            refreshToken,
            accessTokenExpiresAt,
            new AuthUserResponse(user.Id, user.DisplayName, user.Email, user.Role));
    }

    private string GenerateAccessToken(User user, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(_jwtSettings.Secret))
        {
            throw new InvalidOperationException("JWT secret is not configured.");
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("role", user.Role.ToString()),
            new Claim("workspaceId", user.WorkspaceId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<string> GenerateRefreshTokenAsync(User user, string? userAgent, Guid? familyId, CancellationToken cancellationToken)
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshToken = new RefreshToken
        {
            Token = HashToken(rawToken),
            WorkspaceId = user.WorkspaceId,
            UserId = user.Id,
            User = user,
            FamilyId = familyId ?? Guid.NewGuid(),
            UserAgentHash = HashUserAgent(userAgent),
            CreatedAt = _clock.UtcNow,
            ExpiresAt = _clock.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
        };

        await _refreshTokens.AddAsync(refreshToken, cancellationToken);
        return rawToken;
    }

    public static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string? HashUserAgent(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return null;
        }

        return HashToken(userAgent.Trim());
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static string Slugify(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        var builder = new StringBuilder(normalized.Length);
        var previousDash = false;

        foreach (var character in normalized)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                previousDash = false;
                continue;
            }

            if (!previousDash)
            {
                builder.Append('-');
                previousDash = true;
            }
        }

        return builder.ToString().Trim('-');
    }
}
