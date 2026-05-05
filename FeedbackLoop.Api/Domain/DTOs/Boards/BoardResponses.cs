namespace FeedbackLoop.Api.Domain.DTOs.Boards;

public sealed record BoardSummaryResponse(
    Guid Id,
    string Name,
    string Slug,
    int PostCount,
    int OpenPostCount,
    int DonePostCount,
    int VoteCount,
    DateTime CreatedAt);

public sealed record BoardResponse(Guid Id, string Name, string Slug, string? Description, bool IsPublic, DateTime CreatedAt);

public sealed record BoardDetailResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    bool IsPublic,
    DateTime CreatedAt);
