using FeedbackLoop.Api.Domain.Entities;
using FeedbackLoop.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FeedbackLoop.Api.Repositories;

public sealed class EfVoteRepository : IVoteRepository
{
    private readonly AppDbContext _dbContext;

    public EfVoteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Vote?> GetByPostAndUserAsync(Guid postId, string endUserToken, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(endUserToken, out var token))
        {
            return Task.FromResult<Vote?>(null);
        }

        return _dbContext.Votes.FirstOrDefaultAsync(
            vote => vote.PostId == postId && vote.EndUserToken == token,
            cancellationToken);
    }

    public async Task CreateAsync(Vote vote, CancellationToken cancellationToken = default)
    {
        await _dbContext.Votes.AddAsync(vote, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vote = await _dbContext.Votes.FirstOrDefaultAsync(vote => vote.Id == id, cancellationToken);
        if (vote is not null)
        {
            _dbContext.Votes.Remove(vote);
        }
    }

    public Task<int> CountByPostAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Votes.CountAsync(vote => vote.PostId == postId, cancellationToken);
    }
}
