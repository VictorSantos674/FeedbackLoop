using FeedbackLoop.Api.Domain.Entities;
using FeedbackLoop.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FeedbackLoop.Api.Repositories;

public sealed class EfUserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;

    public EfUserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return _dbContext.Users.IgnoreQueryFilters().AnyAsync(user => user.Email == normalizedEmail, cancellationToken);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return _dbContext.Users
            .IgnoreQueryFilters()
            .Include(user => user.Workspace)
            .FirstOrDefaultAsync(user => user.Email == normalizedEmail, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _dbContext.Users.AddAsync(user, cancellationToken);
    }
}
