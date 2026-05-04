using System.ComponentModel.DataAnnotations;

namespace FeedbackLoop.Api.Domain.DTOs.Boards;

public sealed record CreateBoardRequest(
    [property: Required, MaxLength(160)] string Name,
    [property: MaxLength(500)] string? Description,
    bool IsPublic = true);

public sealed record UpdateBoardRequest(
    [property: Required, MaxLength(160)] string Name,
    [property: MaxLength(500)] string? Description,
    bool IsPublic);
