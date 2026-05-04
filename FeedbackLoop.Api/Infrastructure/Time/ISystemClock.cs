namespace FeedbackLoop.Api.Infrastructure.Time;

public interface ISystemClock
{
    DateTime UtcNow { get; }
}
