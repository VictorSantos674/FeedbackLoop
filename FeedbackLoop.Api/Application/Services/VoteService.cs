using FeedbackLoop.Api.Domain.DTOs.Votes;
using FeedbackLoop.Api.Domain.Entities;
using FeedbackLoop.Api.Domain.Exceptions;
using FeedbackLoop.Api.Repositories;

namespace FeedbackLoop.Api.Application.Services;

public sealed class VoteService : IVoteService
{
    private readonly IPostRepository _posts;
    private readonly IVoteRepository _votes;
    private readonly ITransactionRunner _transactionRunner;
    private readonly IUnitOfWork _unitOfWork;

    public VoteService(
        IPostRepository posts,
        IVoteRepository votes,
        ITransactionRunner transactionRunner,
        IUnitOfWork unitOfWork)
    {
        _posts = posts;
        _votes = votes;
        _transactionRunner = transactionRunner;
        _unitOfWork = unitOfWork;
    }

    public Task<VoteResult> ToggleAsync(Guid boardId, Guid postId, string endUserToken, CancellationToken cancellationToken = default)
    {
        return _transactionRunner.RunAsync(async token =>
        {
            if (!Guid.TryParse(endUserToken, out var parsedEndUserToken))
            {
                throw new FeedbackLoop.Api.Domain.Exceptions.ValidationException(new Dictionary<string, string[]>
                {
                    ["endUserToken"] = new[] { "endUserToken must be a valid UUID." }
                });
            }

            var post = await _posts.GetByIdAsync(postId, boardId, token)
                ?? throw new NotFoundException("Post not found.");

            var existingVote = await _votes.GetByPostAndUserAsync(postId, endUserToken, token);
            if (existingVote is not null)
            {
                await _votes.DeleteAsync(existingVote.Id, token);
                post.VoteCount = Math.Max(0, post.VoteCount - 1);
                await _posts.UpdateAsync(post, token);
                await _unitOfWork.SaveChangesAsync(token);
                return new VoteResult(false, post.VoteCount);
            }

            await _votes.CreateAsync(new Vote
            {
                WorkspaceId = post.WorkspaceId,
                PostId = post.Id,
                EndUserToken = parsedEndUserToken,
                CreatedAtUtc = DateTime.UtcNow
            }, token);

            post.VoteCount++;
            await _posts.UpdateAsync(post, token);
            await _unitOfWork.SaveChangesAsync(token);
            return new VoteResult(true, post.VoteCount);
        }, cancellationToken);
    }
}
