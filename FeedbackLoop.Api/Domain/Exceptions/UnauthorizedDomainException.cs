namespace FeedbackLoop.Api.Domain.Exceptions;

public sealed class UnauthorizedDomainException : Exception
{
    public UnauthorizedDomainException(string message = "Invalid credentials.")
        : base(message)
    {
    }
}
