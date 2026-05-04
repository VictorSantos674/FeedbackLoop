using FeedbackLoop.Api.Domain.DTOs.Auth;

namespace FeedbackLoop.Api.Application.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, string? userAgent = null, CancellationToken cancellationToken = default);

    Task<AuthResponse> LoginAsync(LoginRequest request, string? userAgent = null, CancellationToken cancellationToken = default);

    Task<AuthResponse> RefreshAsync(RefreshRequest request, string? userAgent = null, CancellationToken cancellationToken = default);

    Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default);
}
