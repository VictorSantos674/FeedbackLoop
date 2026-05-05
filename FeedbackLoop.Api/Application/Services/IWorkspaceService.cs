using FeedbackLoop.Api.Domain.DTOs.Workspaces;

namespace FeedbackLoop.Api.Application.Services;

public interface IWorkspaceService
{
    Task<WorkspaceResponse> GetCurrentAsync(Guid workspaceId, CancellationToken cancellationToken = default);

    Task<WorkspaceResponse> UpdateNameAsync(Guid workspaceId, string newName, CancellationToken cancellationToken = default);
}
