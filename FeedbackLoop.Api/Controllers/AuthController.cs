using FeedbackLoop.Api.Application.Services;
using FeedbackLoop.Api.Domain.DTOs.Auth;
using FeedbackLoop.Api.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackLoop.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.RegisterAsync(request, GetUserAgent(), cancellationToken);
            return Ok(response);
        }
        catch (ConflictException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.LoginAsync(request, GetUserAgent(), cancellationToken);
            return Ok(response);
        }
        catch (UnauthorizedDomainException)
        {
            _logger.LogWarning("Failed login attempt for email hash {EmailHash}", AuthService.HashToken(request.Email.Trim().ToLowerInvariant()));
            return Unauthorized(new { message = "Invalid credentials." });
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.RefreshAsync(request, GetUserAgent(), cancellationToken);
            return Ok(response);
        }
        catch (UnauthorizedDomainException)
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutRequest request, CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(request, cancellationToken);
        return NoContent();
    }

    private string? GetUserAgent()
    {
        return Request.Headers["User-Agent"].FirstOrDefault();
    }
}
