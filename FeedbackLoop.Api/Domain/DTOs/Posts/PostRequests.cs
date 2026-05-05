using System.ComponentModel.DataAnnotations;
using FeedbackLoop.Api.Domain.Enums;

namespace FeedbackLoop.Api.Domain.DTOs.Posts;

public sealed record CreatePostRequest(
    [Required, MinLength(10), MaxLength(100)] string Title,
    [MaxLength(500)] string? Description,
    [Required] string EndUserToken,
    [MaxLength(160)] string? EndUserName);

public sealed record UpdatePostStatusRequest(PostStatus Status);
