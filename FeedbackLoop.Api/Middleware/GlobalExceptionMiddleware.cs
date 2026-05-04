using System.Net;
using FeedbackLoop.Api.Domain.Exceptions;

namespace FeedbackLoop.Api.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, error, message, errors) = exception switch
        {
            NotFoundException notFound => (HttpStatusCode.NotFound, "not_found", notFound.Message, null),
            ForbiddenException forbidden => (HttpStatusCode.Forbidden, "forbidden", forbidden.Message, null),
            ConflictException conflict => (HttpStatusCode.Conflict, "conflict", conflict.Message, null),
            FeedbackLoop.Api.Domain.Exceptions.ValidationException validation => (HttpStatusCode.UnprocessableEntity, "validation_error", validation.Message, validation.Errors),
            UnauthorizedException unauthorized => (HttpStatusCode.Unauthorized, "unauthorized", unauthorized.Message, null),
            _ => (HttpStatusCode.InternalServerError, "internal_error", "An unexpected error occurred.", null)
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception while processing request.");
        }

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        object response = errors is not null
            ? new { error, message, errors }
            : new
            {
                error,
                message = _environment.IsDevelopment() || statusCode != HttpStatusCode.InternalServerError
                    ? message
                    : "An unexpected error occurred."
            };

        await context.Response.WriteAsJsonAsync(response);
    }
}
