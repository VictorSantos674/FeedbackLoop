namespace FeedbackLoop.Api.Domain.Common;

public interface IWorkspaceScopedEntity
{
    Guid WorkspaceId { get; set; }
}
