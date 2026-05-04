using FeedbackLoop.Api.Domain.Enums;

namespace FeedbackLoop.Api.Domain.DTOs.Posts;

public sealed record PostResponse(
    Guid Id,
    string Title,
    string? Description,
    PostStatus Status,
    int VoteCount,
    int CommentCount,
    bool? HasVoted,
    string EndUserName,
    DateTime CreatedAt);

public sealed record StatusHistoryEntry(PostStatus? From, PostStatus To, string ChangedBy, DateTime ChangedAt);
