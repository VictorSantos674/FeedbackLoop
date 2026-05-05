using FeedbackLoop.Api.Application.Services;
using FeedbackLoop.Api.Domain.DTOs.Common;
using FeedbackLoop.Api.Domain.DTOs.Posts;
using FeedbackLoop.Api.Domain.DTOs.Votes;
using FeedbackLoop.Api.Domain.Enums;
using FeedbackLoop.Api.Domain.Exceptions;
using FeedbackLoop.Api.Domain.Models;
using FeedbackLoop.Api.Domain.Entities;
using FeedbackLoop.Api.Infrastructure.Tenancy;
using FeedbackLoop.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackLoop.Api.Controllers;

[ApiController]
[Route("api/widget/{boardSlug}")]
public sealed class WidgetController : ControllerBase
{
    private readonly IBoardRepository _boards;
    private readonly IPostService _posts;
    private readonly IVoteService _votes;
    private readonly ICurrentWorkspaceContext _workspaceContext;

    public WidgetController(
        IBoardRepository boards,
        IPostService posts,
        IVoteService votes,
        ICurrentWorkspaceContext workspaceContext)
    {
        _boards = boards;
        _posts = posts;
        _votes = votes;
        _workspaceContext = workspaceContext;
    }

    [HttpGet("posts")]
    public async Task<ActionResult<PagedResult<PostResponse>>> GetPosts(
        string boardSlug,
        [FromQuery] string? endUserToken,
        [FromQuery] PostStatus? status,
        [FromQuery] string? search,
        [FromQuery] PostSortBy sortBy = PostSortBy.MostVoted,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var board = await ResolveBoardAndWorkspaceAsync(boardSlug, cancellationToken);

        return Ok(await _posts.GetByBoardAsync(
            board.Id,
            new PostFilter(status, search, sortBy, page, pageSize, endUserToken),
            cancellationToken));
    }

    [HttpPost("posts")]
    public async Task<ActionResult<PostResponse>> CreatePost(string boardSlug, CreatePostRequest request, CancellationToken cancellationToken)
    {
        var board = await ResolveBoardAndWorkspaceAsync(boardSlug, cancellationToken);

        var response = await _posts.CreateAsync(board.Id, request, cancellationToken);
        return CreatedAtAction(nameof(GetPosts), new { boardSlug }, response);
    }

    [HttpPost("posts/{postId:guid}/vote")]
    public async Task<ActionResult<VoteResult>> ToggleVote(
        string boardSlug,
        Guid postId,
        ToggleVoteRequest request,
        CancellationToken cancellationToken)
    {
        var board = await ResolveBoardAndWorkspaceAsync(boardSlug, cancellationToken);

        return Ok(await _votes.ToggleAsync(board.Id, postId, request.EndUserToken, cancellationToken));
    }

    private async Task<Board> ResolveBoardAndWorkspaceAsync(string boardSlug, CancellationToken cancellationToken)
    {
        var board = await _boards.GetBySlugWithWorkspaceAsync(boardSlug, cancellationToken)
            ?? throw new NotFoundException("Board not found.");

        _workspaceContext.SetWorkspace(board.WorkspaceId);
        return board;
    }
}
