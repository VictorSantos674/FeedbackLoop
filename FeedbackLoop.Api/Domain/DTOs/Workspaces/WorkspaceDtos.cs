using System.ComponentModel.DataAnnotations;

namespace FeedbackLoop.Api.Domain.DTOs.Workspaces;

public sealed record WorkspaceResponse(Guid Id, string Name, string Slug);

public sealed record UpdateWorkspaceRequest([Required, MinLength(2), MaxLength(50)] string Name);
