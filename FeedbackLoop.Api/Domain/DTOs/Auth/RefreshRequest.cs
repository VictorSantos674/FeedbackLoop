using System.ComponentModel.DataAnnotations;

namespace FeedbackLoop.Api.Domain.DTOs.Auth;

public sealed record RefreshRequest([property: Required] string RefreshToken);
