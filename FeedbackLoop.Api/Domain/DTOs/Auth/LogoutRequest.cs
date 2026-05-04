using System.ComponentModel.DataAnnotations;

namespace FeedbackLoop.Api.Domain.DTOs.Auth;

public sealed record LogoutRequest([property: Required] string RefreshToken);
