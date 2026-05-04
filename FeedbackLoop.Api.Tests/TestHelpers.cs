using FeedbackLoop.Api.Infrastructure.Tenancy;
using FeedbackLoop.Api.Infrastructure.Time;
using FeedbackLoop.Api.Repositories;

namespace FeedbackLoop.Api.Tests;

internal sealed class FixedClock : ISystemClock
{
    public FixedClock(DateTime utcNow)
    {
        UtcNow = utcNow;
    }

    public DateTime UtcNow { get; }
}

internal sealed class TestWorkspaceContext : ICurrentWorkspaceContext
{
    public TestWorkspaceContext(Guid workspaceId)
    {
        WorkspaceId = workspaceId;
    }

    public Guid? WorkspaceId { get; private set; }

    public void SetWorkspace(Guid workspaceId)
    {
        WorkspaceId = workspaceId;
    }
}

internal sealed class InlineTransactionRunner : ITransactionRunner
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task<T> RunAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await action(cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
