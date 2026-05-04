using System.ComponentModel.DataAnnotations;

namespace FeedbackLoop.Api.Domain.DTOs.Auth;

public sealed record RegisterRequest(
    [property: Required, MaxLength(120)] string Name,
    [property: Required, EmailAddress, MaxLength(254)] string Email,
    [property: Required, MinLength(8), MaxLength(128)] string Password,
    [property: Required, MaxLength(160)] string WorkspaceName);
