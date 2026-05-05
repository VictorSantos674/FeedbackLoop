using FeedbackLoop.Api.Domain.Entities;
using FeedbackLoop.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FeedbackLoop.Api.Repositories;

public sealed class EfStatusHistoryRepository : IStatusHistoryRepository
{
    private readonly AppDbContext _dbContext;

    public EfStatusHistoryRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<StatusHistory>> GetByPostAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.StatusHistory
            .Include(history => history.ChangedByUser)
            .Where(history => history.PostId == postId)
            .OrderBy(history => history.ChangedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task CreateAsync(StatusHistory entry, CancellationToken cancellationToken = default)
    {
        await _dbContext.StatusHistory.AddAsync(entry, cancellationToken);
    }
}
