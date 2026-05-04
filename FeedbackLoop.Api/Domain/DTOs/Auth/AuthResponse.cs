using FeedbackLoop.Api.Domain.Enums;

namespace FeedbackLoop.Api.Domain.DTOs.Auth;

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    AuthUserResponse User);

public sealed record AuthUserResponse(
    Guid Id,
    string Name,
    string Email,
    UserRole Role);
