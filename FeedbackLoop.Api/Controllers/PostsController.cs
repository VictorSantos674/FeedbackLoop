using FeedbackLoop.Api.Application.Services;
using FeedbackLoop.Api.Domain.DTOs.Common;
using FeedbackLoop.Api.Domain.DTOs.Posts;
using FeedbackLoop.Api.Domain.Enums;
using FeedbackLoop.Api.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackLoop.Api.Controllers;

[ApiController]
[Authorize(Policy = "MemberOrAbove")]
[Route("api/boards/{boardId:guid}/posts")]
public sealed class PostsController : ControllerBase
{
    private readonly IPostService _posts;

    public PostsController(IPostService posts)
    {
        _posts = posts;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<PostResponse>>> GetByBoard(
        Guid boardId,
        [FromQuery] PostStatus? status,
        [FromQuery] string? search,
        [FromQuery] PostSortBy sortBy = PostSortBy.MostVoted,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _posts.GetByBoardAsync(boardId, new PostFilter(status, search, sortBy, page, pageSize), cancellationToken));
    }

    [HttpGet("{postId:guid}")]
    public async Task<ActionResult<PostResponse>> GetById(Guid boardId, Guid postId, CancellationToken cancellationToken)
    {
        return Ok(await _posts.GetByIdAsync(boardId, postId, cancellationToken: cancellationToken));
    }

    [HttpPatch("{postId:guid}/status")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<PostResponse>> UpdateStatus(
        Guid boardId,
        Guid postId,
        UpdatePostStatusRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _posts.UpdateStatusAsync(boardId, postId, request.Status, User.GetUserId(), cancellationToken));
    }

    [HttpGet("{postId:guid}/history")]
    public async Task<ActionResult<IReadOnlyList<StatusHistoryEntry>>> GetStatusHistory(Guid boardId, Guid postId, CancellationToken cancellationToken)
    {
        return Ok(await _posts.GetStatusHistoryAsync(boardId, postId, cancellationToken));
    }
}
