using FeedbackLoop.Api.Application.Services;
using FeedbackLoop.Api.Domain.DTOs.Boards;
using FeedbackLoop.Api.Domain.Entities;
using FeedbackLoop.Api.Domain.Enums;
using FeedbackLoop.Api.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace FeedbackLoop.Api.Tests;

public sealed class BoardServiceTests
{
    [Fact]
    public async Task Create_DuplicateSlug_AppendsUniquesSuffix()
    {
        var fixture = new BoardServiceFixture();
        Board? createdBoard = null;

        fixture.Users
            .Setup(repository => repository.GetByIdAsync(fixture.Admin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fixture.Admin);
        fixture.Boards
            .Setup(repository => repository.GetBySlugAsync("meu-produto", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Board { Id = Guid.NewGuid(), WorkspaceId = fixture.WorkspaceId, Name = "Meu Produto", Slug = "meu-produto" });
        fixture.Boards
            .Setup(repository => repository.GetBySlugAsync("meu-produto-2", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Board?)null);
        fixture.Boards
            .Setup(repository => repository.CreateAsync(It.IsAny<Board>(), It.IsAny<CancellationToken>()))
            .Callback<Board, CancellationToken>((board, _) => createdBoard = board)
            .Returns(Task.CompletedTask);

        var response = await fixture.Service.CreateAsync(new CreateBoardRequest("Meu Produto", null), fixture.Admin.Id);

        response.Slug.Should().Be("meu-produto-2");
        createdBoard.Should().NotBeNull();
        createdBoard!.WorkspaceId.Should().Be(fixture.WorkspaceId);
        fixture.UnitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_BoardWithPosts_SoftDeletesBoard()
    {
        var fixture = new BoardServiceFixture();
        var board = new Board
        {
            Id = Guid.NewGuid(),
            WorkspaceId = fixture.WorkspaceId,
            Name = "Roadmap",
            Slug = "roadmap",
            Posts = new List<Post> { new() { Id = Guid.NewGuid(), WorkspaceId = fixture.WorkspaceId, Title = "Long enough title", Description = "Desc" } }
        };

        fixture.Users
            .Setup(repository => repository.GetByIdAsync(fixture.Admin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fixture.Admin);
        fixture.Boards
            .Setup(repository => repository.GetByIdAsync(board.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(board);

        await fixture.Service.DeleteAsync(board.Id, fixture.Admin.Id);

        board.DeletedAt.Should().Be(fixture.Now);
        fixture.Boards.Verify(repository => repository.UpdateAsync(board, It.IsAny<CancellationToken>()), Times.Once);
        fixture.UnitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private sealed class BoardServiceFixture
    {
        public Guid WorkspaceId { get; } = Guid.NewGuid();

        public DateTime Now { get; } = new(2026, 5, 3, 12, 0, 0, DateTimeKind.Utc);

        public User Admin { get; }

        public Mock<IBoardRepository> Boards { get; } = new();

        public Mock<IUserRepository> Users { get; } = new();

        public Mock<IUnitOfWork> UnitOfWork { get; } = new();

        public BoardService Service { get; }

        public BoardServiceFixture()
        {
            Admin = new User
            {
                Id = Guid.NewGuid(),
                WorkspaceId = WorkspaceId,
                Email = "admin@example.com",
                DisplayName = "Admin",
                Role = UserRole.Admin
            };

            Service = new BoardService(
                Boards.Object,
                Users.Object,
                UnitOfWork.Object,
                new TestWorkspaceContext(WorkspaceId),
                new FixedClock(Now));
        }
    }
}
