using FeedbackLoop.Api.Domain.Entities;
using FeedbackLoop.Api.Infrastructure.Persistence;

namespace FeedbackLoop.Api.Repositories;

public sealed class EfWorkspaceRepository : IWorkspaceRepository
{
    private readonly AppDbContext _dbContext;

    public EfWorkspaceRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Workspace workspace, CancellationToken cancellationToken = default)
    {
        await _dbContext.Workspaces.AddAsync(workspace, cancellationToken);
    }
}
