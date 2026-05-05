using FeedbackLoop.Api.Application.Services;
using FeedbackLoop.Api.Domain.DTOs.Boards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackLoop.Api.Controllers;

[ApiController]
[Authorize(Policy = "MemberOrAbove")]
[Route("api/boards")]
public sealed class BoardsController : ControllerBase
{
    private readonly IBoardService _boards;

    public BoardsController(IBoardService boards)
    {
        _boards = boards;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BoardSummaryResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _boards.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BoardDetailResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _boards.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<BoardResponse>> Create(CreateBoardRequest request, CancellationToken cancellationToken)
    {
        var response = await _boards.CreateAsync(request, User.GetUserId(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<BoardDetailResponse>> Update(Guid id, UpdateBoardRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _boards.UpdateAsync(id, request, User.GetUserId(), cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _boards.DeleteAsync(id, User.GetUserId(), cancellationToken);
        return NoContent();
    }
}
