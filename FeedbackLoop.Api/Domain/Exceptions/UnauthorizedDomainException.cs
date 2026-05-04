namespace FeedbackLoop.Api.Domain.Exceptions;

public sealed class UnauthorizedDomainException : UnauthorizedException
{
    public UnauthorizedDomainException(string message = "Invalid credentials.")
        : base(message)
    {
    }
}
