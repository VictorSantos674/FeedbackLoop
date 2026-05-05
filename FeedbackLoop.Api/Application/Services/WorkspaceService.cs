using FeedbackLoop.Api.Domain.DTOs.Workspaces;
using FeedbackLoop.Api.Domain.Exceptions;
using FeedbackLoop.Api.Infrastructure.Time;
using FeedbackLoop.Api.Repositories;

namespace FeedbackLoop.Api.Application.Services;

public sealed class WorkspaceService : IWorkspaceService
{
    private readonly IWorkspaceRepository _workspaces;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISystemClock _clock;

    public WorkspaceService(IWorkspaceRepository workspaces, IUnitOfWork unitOfWork, ISystemClock clock)
    {
        _workspaces = workspaces;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<WorkspaceResponse> GetCurrentAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        var workspace = await _workspaces.GetByIdAsync(workspaceId, cancellationToken)
            ?? throw new NotFoundException("Workspace not found.");

        return new WorkspaceResponse(workspace.Id, workspace.Name, workspace.Slug);
    }

    public async Task<WorkspaceResponse> UpdateNameAsync(Guid workspaceId, string newName, CancellationToken cancellationToken = default)
    {
        var name = newName.Trim();
        if (name.Length < 2 || name.Length > 50)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["name"] = new[] { "Workspace name must be between 2 and 50 characters." }
            });
        }

        var workspace = await _workspaces.GetByIdAsync(workspaceId, cancellationToken)
            ?? throw new NotFoundException("Workspace not found.");

        workspace.Name = name;
        workspace.UpdatedAtUtc = _clock.UtcNow;

        await _workspaces.UpdateAsync(workspace, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new WorkspaceResponse(workspace.Id, workspace.Name, workspace.Slug);
    }
}
