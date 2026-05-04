using FeedbackLoop.Api.Domain.Entities;

namespace FeedbackLoop.Api.Application.Services;

public interface INotificationService
{
    Task NotifyRoadmapStatusChangedAsync(Post post, CancellationToken cancellationToken = default);
}
