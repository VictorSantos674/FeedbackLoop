using System.Security.Claims;

namespace FeedbackLoop.Api.Infrastructure.Tenancy;

public sealed class WorkspaceResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public WorkspaceResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, ICurrentWorkspaceContext currentWorkspaceContext)
    {
        var workspaceClaim = httpContext.User.FindFirstValue("workspaceId");
        if (Guid.TryParse(workspaceClaim, out var workspaceId))
        {
            currentWorkspaceContext.SetWorkspace(workspaceId);
        }

        await _next(httpContext);
    }
}
