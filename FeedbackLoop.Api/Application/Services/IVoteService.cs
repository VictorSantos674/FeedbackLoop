using FeedbackLoop.Api.Domain.DTOs.Votes;

namespace FeedbackLoop.Api.Application.Services;

public interface IVoteService
{
    Task<VoteResult> ToggleAsync(Guid postId, string endUserToken, CancellationToken cancellationToken = default);
}
