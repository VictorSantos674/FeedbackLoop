using FeedbackLoop.Api.Domain.Entities;

namespace FeedbackLoop.Api.Application.Services;

public sealed class LogNotificationService : INotificationService
{
    private readonly ILogger<LogNotificationService> _logger;

    public LogNotificationService(ILogger<LogNotificationService> logger)
    {
        _logger = logger;
    }

    public Task NotifyRoadmapStatusChangedAsync(Post post, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Post {PostId} moved to roadmap status {Status}.", post.Id, post.Status);
        return Task.CompletedTask;
    }
}
