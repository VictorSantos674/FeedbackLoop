using FeedbackLoop.Api.Domain.Entities;

namespace FeedbackLoop.Api.Repositories;

public interface IBoardRepository
{
    Task<IReadOnlyList<Board>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Board?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Board?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<Board?> GetBySlugWithWorkspaceAsync(string slug, CancellationToken cancellationToken = default);

    Task CreateAsync(Board board, CancellationToken cancellationToken = default);

    Task UpdateAsync(Board board, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
