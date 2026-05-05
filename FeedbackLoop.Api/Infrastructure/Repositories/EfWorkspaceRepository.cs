using FeedbackLoop.Api.Domain.Entities;
using FeedbackLoop.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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

    public Task<Workspace?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Workspaces.FirstOrDefaultAsync(workspace => workspace.Id == id, cancellationToken);
    }

    public Task UpdateAsync(Workspace workspace, CancellationToken cancellationToken = default)
    {
        _dbContext.Workspaces.Update(workspace);
        return Task.CompletedTask;
    }
}
