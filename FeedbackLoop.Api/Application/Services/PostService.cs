using FeedbackLoop.Api.Domain.DTOs.Common;
using FeedbackLoop.Api.Domain.DTOs.Posts;
using FeedbackLoop.Api.Domain.Entities;
using FeedbackLoop.Api.Domain.Enums;
using FeedbackLoop.Api.Domain.Exceptions;
using FeedbackLoop.Api.Domain.Models;
using FeedbackLoop.Api.Infrastructure.Time;
using FeedbackLoop.Api.Repositories;

namespace FeedbackLoop.Api.Application.Services;

public sealed class PostService : IPostService
{
    private static readonly PostStatus[] NotifiableStatuses =
    {
        PostStatus.Planned,
        PostStatus.InProgress,
        PostStatus.Done
    };

    private readonly IBoardRepository _boards;
    private readonly IPostRepository _posts;
    private readonly IVoteRepository _votes;
    private readonly IUserRepository _users;
    private readonly IStatusHistoryRepository _statusHistory;
    private readonly INotificationService _notifications;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISystemClock _clock;

    public PostService(
        IBoardRepository boards,
        IPostRepository posts,
        IVoteRepository votes,
        IUserRepository users,
        IStatusHistoryRepository statusHistory,
        INotificationService notifications,
        IUnitOfWork unitOfWork,
        ISystemClock clock)
    {
        _boards = boards;
        _posts = posts;
        _votes = votes;
        _users = users;
        _statusHistory = statusHistory;
        _notifications = notifications;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<PagedResult<PostResponse>> GetByBoardAsync(Guid boardId, PostFilter filter, CancellationToken cancellationToken = default)
    {
        _ = await _boards.GetByIdAsync(boardId, cancellationToken)
            ?? throw new NotFoundException("Board not found.");

        var posts = await _posts.GetByBoardAsync(boardId, filter, cancellationToken);
        var totalCount = await _posts.CountByBoardAsync(boardId, filter, cancellationToken);
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        var responses = new List<PostResponse>();
        foreach (var post in posts)
        {
            responses.Add(await ToResponseAsync(post, filter.EndUserToken, cancellationToken));
        }

        return new PagedResult<PostResponse>(responses, totalCount, page, pageSize, totalPages);
    }

    public async Task<PostResponse> GetByIdAsync(Guid boardId, Guid postId, string? endUserToken = null, CancellationToken cancellationToken = default)
    {
        var post = await _posts.GetByIdAsync(postId, boardId, cancellationToken)
            ?? throw new NotFoundException("Post not found.");

        return await ToResponseAsync(post, endUserToken, cancellationToken);
    }

    public async Task<PostResponse> CreateAsync(Guid boardId, CreatePostRequest request, CancellationToken cancellationToken = default)
    {
        ValidateCreatePost(request);

        if (!Guid.TryParse(request.EndUserToken, out var endUserToken))
        {
            throw new FeedbackLoop.Api.Domain.Exceptions.ValidationException(new Dictionary<string, string[]>
            {
                ["endUserToken"] = new[] { "endUserToken must be a valid UUID." }
            });
        }

        var board = await _boards.GetByIdAsync(boardId, cancellationToken)
            ?? throw new NotFoundException("Board not found.");

        var startOfDay = _clock.UtcNow.Date;
        var postCountToday = await _posts.CountCreatedByEndUserOnBoardSinceAsync(boardId, endUserToken, startOfDay, cancellationToken);
        if (postCountToday >= 10)
        {
            throw new FeedbackLoop.Api.Domain.Exceptions.ValidationException(new Dictionary<string, string[]>
            {
                ["endUserToken"] = new[] { "Daily post limit reached for this board." }
            });
        }

        var post = new Post
        {
            WorkspaceId = board.WorkspaceId,
            BoardId = board.Id,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            EndUserToken = endUserToken,
            EndUserName = request.EndUserName?.Trim(),
            Status = PostStatus.Open,
            CreatedAtUtc = _clock.UtcNow
        };

        await _posts.CreateAsync(post, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToResponseAsync(post, request.EndUserToken, cancellationToken);
    }

    public async Task<PostResponse> UpdateStatusAsync(Guid boardId, Guid postId, PostStatus newStatus, Guid adminUserId, CancellationToken cancellationToken = default)
    {
        var admin = await _users.GetByIdAsync(adminUserId, cancellationToken)
            ?? throw new ForbiddenException();

        if (admin.Role != UserRole.Admin)
        {
            throw new ForbiddenException();
        }

        var post = await _posts.GetByIdAsync(postId, boardId, cancellationToken)
            ?? throw new NotFoundException("Post not found.");

        if (post.Status != newStatus)
        {
            var previousStatus = post.Status;
            post.Status = newStatus;
            post.UpdatedAtUtc = _clock.UtcNow;

            await _statusHistory.CreateAsync(new StatusHistory
            {
                WorkspaceId = post.WorkspaceId,
                PostId = post.Id,
                FromStatus = previousStatus,
                ToStatus = newStatus,
                ChangedByUserId = admin.Id,
                ChangedAtUtc = _clock.UtcNow
            }, cancellationToken);

            await _posts.UpdateAsync(post, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (NotifiableStatuses.Contains(newStatus))
            {
                await _notifications.NotifyRoadmapStatusChangedAsync(post, cancellationToken);
            }
        }

        return await ToResponseAsync(post, null, cancellationToken);
    }

    public async Task<IReadOnlyList<StatusHistoryEntry>> GetStatusHistoryAsync(Guid boardId, Guid postId, CancellationToken cancellationToken = default)
    {
        _ = await _posts.GetByIdAsync(postId, boardId, cancellationToken)
            ?? throw new NotFoundException("Post not found.");

        var history = await _statusHistory.GetByPostAsync(postId, cancellationToken);
        return history
            .Select(entry => new StatusHistoryEntry(
                entry.FromStatus,
                entry.ToStatus,
                entry.ChangedByUser.DisplayName,
                entry.ChangedAtUtc))
            .ToList();
    }

    private async Task<PostResponse> ToResponseAsync(Post post, string? endUserToken, CancellationToken cancellationToken)
    {
        bool? hasVoted = null;
        if (!string.IsNullOrWhiteSpace(endUserToken))
        {
            hasVoted = await _votes.GetByPostAndUserAsync(post.Id, endUserToken, cancellationToken) is not null;
        }

        return new PostResponse(
            post.Id,
            post.Title,
            post.Description,
            post.Status,
            post.VoteCount,
            post.Comments.Count,
            hasVoted,
            post.EndUserName ?? "Anonymous",
            post.CreatedAtUtc);
    }

    private static void ValidateCreatePost(CreatePostRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Trim().Length < 10 || request.Title.Trim().Length > 100)
        {
            errors["title"] = new[] { "Title must be between 10 and 100 characters." };
        }

        if (!string.IsNullOrEmpty(request.Description) && request.Description.Length > 500)
        {
            errors["description"] = new[] { "Description must be 500 characters or fewer." };
        }

        if (string.IsNullOrWhiteSpace(request.EndUserToken))
        {
            errors["endUserToken"] = new[] { "endUserToken is required." };
        }

        if (errors.Count > 0)
        {
            throw new FeedbackLoop.Api.Domain.Exceptions.ValidationException(errors);
        }
    }
}
