using FeedbackLoop.Api.Domain.Entities;

namespace FeedbackLoop.Api.Repositories;

public interface IWorkspaceRepository
{
    Task<Workspace?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Workspace workspace, CancellationToken cancellationToken = default);

    Task UpdateAsync(Workspace workspace, CancellationToken cancellationToken = default);
}
