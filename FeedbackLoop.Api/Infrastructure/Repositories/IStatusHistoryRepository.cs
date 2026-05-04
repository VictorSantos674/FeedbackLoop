using FeedbackLoop.Api.Domain.Entities;

namespace FeedbackLoop.Api.Repositories;

public interface IStatusHistoryRepository
{
    Task<IReadOnlyList<StatusHistory>> GetByPostAsync(Guid postId, CancellationToken cancellationToken = default);

    Task CreateAsync(StatusHistory entry, CancellationToken cancellationToken = default);
}
