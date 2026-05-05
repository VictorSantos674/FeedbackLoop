using FeedbackLoop.Api.Application.Services;
using FeedbackLoop.Api.Domain.DTOs.Posts;
using FeedbackLoop.Api.Domain.Entities;
using FeedbackLoop.Api.Domain.Enums;
using FeedbackLoop.Api.Domain.Exceptions;
using FeedbackLoop.Api.Repositories;
using FluentAssertions;
using Moq;
using Xunit;
using ValidationException = FeedbackLoop.Api.Domain.Exceptions.ValidationException;

namespace FeedbackLoop.Api.Tests;

public sealed class PostServiceTests
{
    [Fact]
    public async Task Create_ValidPost_ReturnsPostResponse()
    {
        var fixture = new PostServiceFixture();
        var endUserToken = Guid.NewGuid();
        Post? createdPost = null;

        fixture.Boards.Setup(repository => repository.GetByIdAsync(fixture.Board.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fixture.Board);
        fixture.Posts.Setup(repository => repository.CountCreatedByEndUserOnBoardSinceAsync(fixture.Board.Id, endUserToken, fixture.Now.Date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        fixture.Posts.Setup(repository => repository.CreateAsync(It.IsAny<Post>(), It.IsAny<CancellationToken>()))
            .Callback<Post, CancellationToken>((post, _) => createdPost = post)
            .Returns(Task.CompletedTask);

        var response = await fixture.Service.CreateAsync(
            fixture.Board.Id,
            new CreatePostRequest("Adicionar exportacao CSV", "Seria util exportar dados.", endUserToken.ToString(), "Cliente"));

        response.Title.Should().Be("Adicionar exportacao CSV");
        response.HasVoted.Should().BeFalse();
        createdPost.Should().NotBeNull();
        createdPost!.WorkspaceId.Should().Be(fixture.WorkspaceId);
    }

    [Fact]
    public async Task Create_TitleTooShort_ThrowsValidationException()
    {
        var fixture = new PostServiceFixture();

        var act = () => fixture.Service.CreateAsync(
            fixture.Board.Id,
            new CreatePostRequest("Curto", null, Guid.NewGuid().ToString(), "Cliente"));

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Create_ExceedsSpamLimit_ThrowsValidationException()
    {
        var fixture = new PostServiceFixture();
        var endUserToken = Guid.NewGuid();

        fixture.Boards.Setup(repository => repository.GetByIdAsync(fixture.Board.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fixture.Board);
        fixture.Posts.Setup(repository => repository.CountCreatedByEndUserOnBoardSinceAsync(fixture.Board.Id, endUserToken, fixture.Now.Date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        var act = () => fixture.Service.CreateAsync(
            fixture.Board.Id,
            new CreatePostRequest("Adicionar exportacao CSV", null, endUserToken.ToString(), "Cliente"));

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateStatus_NonAdmin_ThrowsForbiddenException()
    {
        var fixture = new PostServiceFixture();
        var member = fixture.CreateUser(UserRole.Member);

        fixture.Users.Setup(repository => repository.GetByIdAsync(member.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        var act = () => fixture.Service.UpdateStatusAsync(fixture.Board.Id, fixture.Post.Id, PostStatus.Planned, member.Id);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task UpdateStatus_ValidTransition_CreatesStatusHistoryEntry()
    {
        var fixture = new PostServiceFixture();
        var admin = fixture.CreateUser(UserRole.Admin);
        StatusHistory? createdHistory = null;

        fixture.Users.Setup(repository => repository.GetByIdAsync(admin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);
        fixture.Posts.Setup(repository => repository.GetByIdAsync(fixture.Post.Id, fixture.Board.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fixture.Post);
        fixture.StatusHistory.Setup(repository => repository.CreateAsync(It.IsAny<StatusHistory>(), It.IsAny<CancellationToken>()))
            .Callback<StatusHistory, CancellationToken>((history, _) => createdHistory = history)
            .Returns(Task.CompletedTask);

        var response = await fixture.Service.UpdateStatusAsync(fixture.Board.Id, fixture.Post.Id, PostStatus.Planned, admin.Id);

        response.Status.Should().Be(PostStatus.Planned);
        createdHistory.Should().NotBeNull();
        createdHistory!.FromStatus.Should().Be(PostStatus.Open);
        createdHistory.ToStatus.Should().Be(PostStatus.Planned);
        createdHistory.ChangedByUserId.Should().Be(admin.Id);
        fixture.Notifications.Verify(service => service.NotifyRoadmapStatusChangedAsync(fixture.Post, It.IsAny<CancellationToken>()), Times.Once);
    }

    private sealed class PostServiceFixture
    {
        public Guid WorkspaceId { get; } = Guid.NewGuid();

        public DateTime Now { get; } = new(2026, 5, 3, 12, 0, 0, DateTimeKind.Utc);

        public Board Board { get; }

        public Post Post { get; }

        public Mock<IBoardRepository> Boards { get; } = new();

        public Mock<IPostRepository> Posts { get; } = new();

        public Mock<IVoteRepository> Votes { get; } = new();

        public Mock<IUserRepository> Users { get; } = new();

        public Mock<IStatusHistoryRepository> StatusHistory { get; } = new();

        public Mock<INotificationService> Notifications { get; } = new();

        public Mock<IUnitOfWork> UnitOfWork { get; } = new();

        public PostService Service { get; }

        public PostServiceFixture()
        {
            Board = new Board { Id = Guid.NewGuid(), WorkspaceId = WorkspaceId, Name = "Roadmap", Slug = "roadmap" };
            Post = new Post
            {
                Id = Guid.NewGuid(),
                WorkspaceId = WorkspaceId,
                BoardId = Board.Id,
                Title = "Adicionar exportacao CSV",
                Description = "Desc",
                Status = PostStatus.Open
            };

            Votes.Setup(repository => repository.GetByPostAndUserAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Vote?)null);

            Service = new PostService(
                Boards.Object,
                Posts.Object,
                Votes.Object,
                Users.Object,
                StatusHistory.Object,
                Notifications.Object,
                UnitOfWork.Object,
                new FixedClock(Now));
        }

        public User CreateUser(UserRole role)
        {
            return new User
            {
                Id = Guid.NewGuid(),
                WorkspaceId = WorkspaceId,
                Email = $"{role}@example.com",
                DisplayName = role.ToString(),
                Role = role
            };
        }
    }
}
