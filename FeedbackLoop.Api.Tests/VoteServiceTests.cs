using FeedbackLoop.Api.Application.Services;
using FeedbackLoop.Api.Domain.Entities;
using FeedbackLoop.Api.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace FeedbackLoop.Api.Tests;

public sealed class VoteServiceTests
{
    [Fact]
    public async Task Toggle_FirstVote_CreatesVoteAndReturnsVotedTrue()
    {
        var fixture = new VoteServiceFixture();
        var endUserToken = Guid.NewGuid().ToString();

        var result = await fixture.Service.ToggleAsync(fixture.Post.Id, endUserToken);

        result.Voted.Should().BeTrue();
        result.VoteCount.Should().Be(1);
        fixture.VotesList.Should().ContainSingle();
        fixture.Post.VoteCount.Should().Be(1);
    }

    [Fact]
    public async Task Toggle_SecondVote_RemovesVoteAndReturnsVotedFalse()
    {
        var fixture = new VoteServiceFixture();
        var endUserToken = Guid.NewGuid().ToString();
        fixture.VotesList.Add(new Vote { Id = Guid.NewGuid(), WorkspaceId = fixture.WorkspaceId, PostId = fixture.Post.Id, EndUserToken = Guid.Parse(endUserToken) });
        fixture.Post.VoteCount = 1;

        var result = await fixture.Service.ToggleAsync(fixture.Post.Id, endUserToken);

        result.Voted.Should().BeFalse();
        result.VoteCount.Should().Be(0);
        fixture.VotesList.Should().BeEmpty();
    }

    [Fact]
    public async Task Toggle_ConcurrentVotes_DoesNotDuplicateVote()
    {
        var fixture = new VoteServiceFixture();
        var endUserToken = Guid.NewGuid().ToString();

        await Task.WhenAll(
            fixture.Service.ToggleAsync(fixture.Post.Id, endUserToken),
            fixture.Service.ToggleAsync(fixture.Post.Id, endUserToken));

        fixture.VotesList.Count.Should().BeLessThanOrEqualTo(1);
        fixture.VotesList.Select(vote => vote.EndUserToken).Distinct().Count().Should().Be(fixture.VotesList.Count);
    }

    private sealed class VoteServiceFixture
    {
        public Guid WorkspaceId { get; } = Guid.NewGuid();

        public Post Post { get; }

        public List<Vote> VotesList { get; } = new();

        public Mock<IPostRepository> Posts { get; } = new();

        public Mock<IVoteRepository> Votes { get; } = new();

        public Mock<IUnitOfWork> UnitOfWork { get; } = new();

        public VoteService Service { get; }

        public VoteServiceFixture()
        {
            Post = new Post
            {
                Id = Guid.NewGuid(),
                WorkspaceId = WorkspaceId,
                Title = "Adicionar exportacao CSV",
                Description = "Desc"
            };

            Posts.Setup(repository => repository.GetByIdAsync(Post.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Post);
            Posts.Setup(repository => repository.UpdateAsync(Post, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            Votes.Setup(repository => repository.GetByPostAndUserAsync(Post.Id, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns<Guid, string, CancellationToken>((_, token, _) =>
                {
                    var parsedToken = Guid.Parse(token);
                    return Task.FromResult(VotesList.FirstOrDefault(vote => vote.PostId == Post.Id && vote.EndUserToken == parsedToken));
                });
            Votes.Setup(repository => repository.CreateAsync(It.IsAny<Vote>(), It.IsAny<CancellationToken>()))
                .Callback<Vote, CancellationToken>((vote, _) => VotesList.Add(vote))
                .Returns(Task.CompletedTask);
            Votes.Setup(repository => repository.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Callback<Guid, CancellationToken>((id, _) => VotesList.RemoveAll(vote => vote.Id == id))
                .Returns(Task.CompletedTask);

            Service = new VoteService(Posts.Object, Votes.Object, new InlineTransactionRunner(), UnitOfWork.Object);
        }
    }
}
