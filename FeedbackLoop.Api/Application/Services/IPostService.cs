using FeedbackLoop.Api.Domain.DTOs.Common;
using FeedbackLoop.Api.Domain.DTOs.Posts;
using FeedbackLoop.Api.Domain.Models;

namespace FeedbackLoop.Api.Application.Services;

public interface IPostService
{
    Task<PagedResult<PostResponse>> GetByBoardAsync(Guid boardId, PostFilter filter, CancellationToken cancellationToken = default);

    Task<PostResponse> GetByIdAsync(Guid boardId, Guid postId, string? endUserToken = null, CancellationToken cancellationToken = default);

    Task<PostResponse> CreateAsync(Guid boardId, CreatePostRequest request, CancellationToken cancellationToken = default);

    Task<PostResponse> UpdateStatusAsync(Guid boardId, Guid postId, Domain.Enums.PostStatus newStatus, Guid adminUserId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StatusHistoryEntry>> GetStatusHistoryAsync(Guid boardId, Guid postId, CancellationToken cancellationToken = default);
}
