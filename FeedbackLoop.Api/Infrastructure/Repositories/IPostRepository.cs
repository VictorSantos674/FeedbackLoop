using FeedbackLoop.Api.Domain.Entities;
using FeedbackLoop.Api.Domain.Models;

namespace FeedbackLoop.Api.Repositories;

public interface IPostRepository
{
    Task<IReadOnlyList<Post>> GetByBoardAsync(Guid boardId, PostFilter filter, CancellationToken cancellationToken = default);

    Task<int> CountByBoardAsync(Guid boardId, PostFilter filter, CancellationToken cancellationToken = default);

    Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task CreateAsync(Post post, CancellationToken cancellationToken = default);

    Task UpdateAsync(Post post, CancellationToken cancellationToken = default);

    Task<int> CountCreatedByEndUserOnBoardSinceAsync(Guid boardId, Guid endUserToken, DateTime sinceUtc, CancellationToken cancellationToken = default);
}
