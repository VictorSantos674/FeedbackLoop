using FeedbackLoop.Api.Domain.Entities;

namespace FeedbackLoop.Api.Repositories;

public interface IWorkspaceRepository
{
    Task AddAsync(Workspace workspace, CancellationToken cancellationToken = default);
}
