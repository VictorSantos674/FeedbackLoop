using FeedbackLoop.Api.Domain.Entities;
using FeedbackLoop.Api.Domain.Models;
using FeedbackLoop.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FeedbackLoop.Api.Repositories;

public sealed class EfPostRepository : IPostRepository
{
    private readonly AppDbContext _dbContext;

    public EfPostRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Post>> GetByBoardAsync(Guid boardId, PostFilter filter, CancellationToken cancellationToken = default)
    {
        return await ApplyFilter(_dbContext.Posts.AsQueryable(), boardId, filter)
            .Include(post => post.Comments)
            .Skip((NormalizePage(filter.Page) - 1) * NormalizePageSize(filter.PageSize))
            .Take(NormalizePageSize(filter.PageSize))
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountByBoardAsync(Guid boardId, PostFilter filter, CancellationToken cancellationToken = default)
    {
        return ApplyFilter(_dbContext.Posts.AsQueryable(), boardId, filter).CountAsync(cancellationToken);
    }

    public Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Posts
            .Include(post => post.Comments)
            .FirstOrDefaultAsync(post => post.Id == id, cancellationToken);
    }

    public async Task CreateAsync(Post post, CancellationToken cancellationToken = default)
    {
        await _dbContext.Posts.AddAsync(post, cancellationToken);
    }

    public Task UpdateAsync(Post post, CancellationToken cancellationToken = default)
    {
        _dbContext.Posts.Update(post);
        return Task.CompletedTask;
    }

    public Task<int> CountCreatedByEndUserOnBoardSinceAsync(Guid boardId, Guid endUserToken, DateTime sinceUtc, CancellationToken cancellationToken = default)
    {
        return _dbContext.Posts.CountAsync(
            post => post.BoardId == boardId && post.EndUserToken == endUserToken && post.CreatedAtUtc >= sinceUtc,
            cancellationToken);
    }

    private static IQueryable<Post> ApplyFilter(IQueryable<Post> query, Guid boardId, PostFilter filter)
    {
        query = query.Where(post => post.BoardId == boardId);

        if (filter.Status.HasValue)
        {
            query = query.Where(post => post.Status == filter.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLowerInvariant();
            query = query.Where(post =>
                post.Title.ToLower().Contains(search)
                || post.Description.ToLower().Contains(search));
        }

        return filter.SortBy switch
        {
            PostSortBy.Newest => query.OrderByDescending(post => post.CreatedAtUtc),
            PostSortBy.Oldest => query.OrderBy(post => post.CreatedAtUtc),
            PostSortBy.RecentlyUpdated => query.OrderByDescending(post => post.UpdatedAtUtc ?? post.CreatedAtUtc),
            _ => query.OrderByDescending(post => post.VoteCount).ThenByDescending(post => post.CreatedAtUtc)
        };
    }

    private static int NormalizePage(int page)
    {
        return Math.Max(1, page);
    }

    private static int NormalizePageSize(int pageSize)
    {
        return Math.Clamp(pageSize, 1, 100);
    }
}
