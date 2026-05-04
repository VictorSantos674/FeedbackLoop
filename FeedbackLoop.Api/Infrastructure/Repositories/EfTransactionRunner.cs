using FeedbackLoop.Api.Infrastructure.Persistence;

namespace FeedbackLoop.Api.Repositories;

public sealed class EfTransactionRunner : ITransactionRunner
{
    private readonly AppDbContext _dbContext;

    public EfTransactionRunner(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<T> RunAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var result = await action(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return result;
    }
}
