namespace FeedbackLoop.Api.Domain.Exceptions;

public sealed class ValidationException : Exception
{
    public ValidationException(string message)
        : this(new Dictionary<string, string[]> { ["general"] = new[] { message } })
    {
    }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("Validation failed.")
    {
        Errors = errors;
    }

    public IDictionary<string, string[]> Errors { get; }
}
