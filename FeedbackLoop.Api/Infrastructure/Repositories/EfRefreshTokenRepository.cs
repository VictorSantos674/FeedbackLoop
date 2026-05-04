using FeedbackLoop.Api.Domain.Entities;
using FeedbackLoop.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FeedbackLoop.Api.Repositories;

public sealed class EfRefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _dbContext;

    public EfRefreshTokenRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return _dbContext.RefreshTokens
            .Include(token => token.User)
            .ThenInclude(user => user.Workspace)
            .FirstOrDefaultAsync(token => token.Token == tokenHash, cancellationToken);
    }

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    public async Task RevokeActiveTokensForDeviceAsync(Guid userId, string? userAgentHash, DateTime revokedAt, CancellationToken cancellationToken = default)
    {
        var activeTokens = await _dbContext.RefreshTokens
            .Where(token =>
                token.UserId == userId
                && token.UserAgentHash == userAgentHash
                && token.RevokedAt == null
                && token.ExpiresAt > revokedAt)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.RevokedAt = revokedAt;
        }
    }

    public async Task RevokeTokenFamilyAsync(Guid userId, Guid familyId, DateTime revokedAt, CancellationToken cancellationToken = default)
    {
        var familyTokens = await _dbContext.RefreshTokens
            .Where(token => token.UserId == userId && token.FamilyId == familyId && token.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in familyTokens)
        {
            token.RevokedAt = revokedAt;
        }
    }
}
