using FeedbackLoop.Api.Domain.Entities;
using FeedbackLoop.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FeedbackLoop.Api.Repositories;

public sealed class EfBoardRepository : IBoardRepository
{
    private readonly AppDbContext _dbContext;

    public EfBoardRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Board>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Boards
            .Include(board => board.Posts)
            .OrderBy(board => board.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<Board?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Boards
            .Include(board => board.Posts)
            .FirstOrDefaultAsync(board => board.Id == id, cancellationToken);
    }

    public Task<Board?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return _dbContext.Boards.FirstOrDefaultAsync(board => board.Slug == slug, cancellationToken);
    }

    public async Task CreateAsync(Board board, CancellationToken cancellationToken = default)
    {
        await _dbContext.Boards.AddAsync(board, cancellationToken);
    }

    public Task UpdateAsync(Board board, CancellationToken cancellationToken = default)
    {
        _dbContext.Boards.Update(board);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var board = await GetByIdAsync(id, cancellationToken);
        if (board is not null)
        {
            board.DeletedAt = DateTime.UtcNow;
        }
    }
}
