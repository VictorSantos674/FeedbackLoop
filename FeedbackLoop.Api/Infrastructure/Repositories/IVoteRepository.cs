using FeedbackLoop.Api.Domain.Entities;

namespace FeedbackLoop.Api.Repositories;

public interface IVoteRepository
{
    Task<Vote?> GetByPostAndUserAsync(Guid postId, string endUserToken, CancellationToken cancellationToken = default);

    Task CreateAsync(Vote vote, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<int> CountByPostAsync(Guid postId, CancellationToken cancellationToken = default);
}
