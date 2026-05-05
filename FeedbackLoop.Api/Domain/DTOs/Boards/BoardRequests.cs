using System.ComponentModel.DataAnnotations;

namespace FeedbackLoop.Api.Domain.DTOs.Boards;

public sealed record CreateBoardRequest(
    [Required, MaxLength(160)] string Name,
    [MaxLength(500)] string? Description,
    bool IsPublic = true);

public sealed record UpdateBoardRequest(
    [Required, MaxLength(160)] string Name,
    [MaxLength(500)] string? Description,
    bool IsPublic);
