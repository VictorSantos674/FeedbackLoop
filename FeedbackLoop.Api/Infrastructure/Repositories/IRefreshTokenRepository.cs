using FeedbackLoop.Api.Domain.Entities;

namespace FeedbackLoop.Api.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    Task RevokeActiveTokensForDeviceAsync(Guid userId, string? userAgentHash, DateTime revokedAt, CancellationToken cancellationToken = default);

    Task RevokeTokenFamilyAsync(Guid userId, Guid familyId, DateTime revokedAt, CancellationToken cancellationToken = default);
}
