namespace FeedbackLoop.Api.Infrastructure.Time;

public sealed class SystemClock : ISystemClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
