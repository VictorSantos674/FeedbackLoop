using System.ComponentModel.DataAnnotations;
using FeedbackLoop.Api.Domain.Enums;

namespace FeedbackLoop.Api.Domain.DTOs.Posts;

public sealed record CreatePostRequest(
    [property: Required, MinLength(10), MaxLength(100)] string Title,
    [property: MaxLength(500)] string? Description,
    [property: Required] string EndUserToken,
    [property: MaxLength(160)] string? EndUserName);

public sealed record UpdatePostStatusRequest(PostStatus Status);
