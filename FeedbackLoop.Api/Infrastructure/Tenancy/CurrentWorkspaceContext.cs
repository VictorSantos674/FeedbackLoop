namespace FeedbackLoop.Api.Infrastructure.Tenancy;

public sealed class CurrentWorkspaceContext : ICurrentWorkspaceContext
{
    public Guid? WorkspaceId { get; private set; }

    public void SetWorkspace(Guid workspaceId)
    {
        WorkspaceId = workspaceId;
    }
}
