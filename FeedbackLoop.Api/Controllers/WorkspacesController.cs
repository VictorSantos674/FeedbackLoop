using FeedbackLoop.Api.Application.Services;
using FeedbackLoop.Api.Domain.DTOs.Workspaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackLoop.Api.Controllers;

[ApiController]
[Authorize(Policy = "MemberOrAbove")]
[Route("api/workspaces")]
public sealed class WorkspacesController : ControllerBase
{
    private readonly IWorkspaceService _workspaces;

    public WorkspacesController(IWorkspaceService workspaces)
    {
        _workspaces = workspaces;
    }

    [HttpGet("current")]
    public async Task<ActionResult<WorkspaceResponse>> GetCurrent(CancellationToken cancellationToken)
    {
        return Ok(await _workspaces.GetCurrentAsync(User.GetWorkspaceId(), cancellationToken));
    }

    [HttpPatch("current")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<WorkspaceResponse>> UpdateCurrent(UpdateWorkspaceRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _workspaces.UpdateNameAsync(User.GetWorkspaceId(), request.Name, cancellationToken));
    }
}
