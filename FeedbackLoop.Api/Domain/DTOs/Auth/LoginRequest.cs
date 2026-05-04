using System.ComponentModel.DataAnnotations;

namespace FeedbackLoop.Api.Domain.DTOs.Auth;

public sealed record LoginRequest(
    [property: Required, EmailAddress, MaxLength(254)] string Email,
    [property: Required, MaxLength(128)] string Password);
