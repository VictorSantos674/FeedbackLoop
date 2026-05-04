namespace FeedbackLoop.Api.Infrastructure.Tenancy;

public interface ICurrentWorkspaceContext
{
    Guid? WorkspaceId { get; }

    void SetWorkspace(Guid workspaceId);
}
