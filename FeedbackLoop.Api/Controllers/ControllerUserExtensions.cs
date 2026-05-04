using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FeedbackLoop.Api.Domain.Exceptions;

namespace FeedbackLoop.Api.Controllers;

internal static class ControllerUserExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(value, out var userId)
            ? userId
            : throw new UnauthorizedException();
    }
}
