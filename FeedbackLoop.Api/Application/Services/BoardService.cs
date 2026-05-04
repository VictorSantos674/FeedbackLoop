using System.Text;
using FeedbackLoop.Api.Domain.DTOs.Boards;
using FeedbackLoop.Api.Domain.Entities;
using FeedbackLoop.Api.Domain.Enums;
using FeedbackLoop.Api.Domain.Exceptions;
using FeedbackLoop.Api.Infrastructure.Tenancy;
using FeedbackLoop.Api.Infrastructure.Time;
using FeedbackLoop.Api.Repositories;

namespace FeedbackLoop.Api.Application.Services;

public sealed class BoardService : IBoardService
{
    private readonly IBoardRepository _boards;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentWorkspaceContext _workspaceContext;
    private readonly ISystemClock _clock;

    public BoardService(
        IBoardRepository boards,
        IUserRepository users,
        IUnitOfWork unitOfWork,
        ICurrentWorkspaceContext workspaceContext,
        ISystemClock clock)
    {
        _boards = boards;
        _users = users;
        _unitOfWork = unitOfWork;
        _workspaceContext = workspaceContext;
        _clock = clock;
    }

    public async Task<BoardResponse> CreateAsync(CreateBoardRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        await EnsureAdminAsync(userId, cancellationToken);
        var workspaceId = _workspaceContext.WorkspaceId ?? throw new ForbiddenException("Workspace context is missing.");
        var slug = await GenerateUniqueSlugAsync(request.Name, cancellationToken);

        var board = new Board
        {
            WorkspaceId = workspaceId,
            Name = request.Name.Trim(),
            Slug = slug,
            Description = request.Description?.Trim(),
            IsPublic = request.IsPublic,
            CreatedAtUtc = _clock.UtcNow
        };

        await _boards.CreateAsync(board, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(board);
    }

    public async Task<IReadOnlyList<BoardSummaryResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var boards = await _boards.GetAllAsync(cancellationToken);
        return boards
            .Select(board => new BoardSummaryResponse(board.Id, board.Name, board.Slug, board.Posts.Count, board.CreatedAtUtc))
            .ToList();
    }

    public async Task<BoardDetailResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var board = await _boards.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Board not found.");

        return new BoardDetailResponse(board.Id, board.Name, board.Slug, board.Description, board.IsPublic, board.CreatedAtUtc);
    }

    public async Task<BoardDetailResponse> UpdateAsync(Guid id, UpdateBoardRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        await EnsureAdminAsync(userId, cancellationToken);
        var board = await _boards.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Board not found.");

        board.Name = request.Name.Trim();
        board.Description = request.Description?.Trim();
        board.IsPublic = request.IsPublic;
        board.UpdatedAtUtc = _clock.UtcNow;

        await _boards.UpdateAsync(board, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new BoardDetailResponse(board.Id, board.Name, board.Slug, board.Description, board.IsPublic, board.CreatedAtUtc);
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        await EnsureAdminAsync(userId, cancellationToken);
        var board = await _boards.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Board not found.");

        board.DeletedAt = _clock.UtcNow;
        board.UpdatedAtUtc = _clock.UtcNow;
        await _boards.UpdateAsync(board, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureAdminAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken)
            ?? throw new ForbiddenException();

        if (user.Role != UserRole.Admin)
        {
            throw new ForbiddenException();
        }
    }

    private async Task<string> GenerateUniqueSlugAsync(string name, CancellationToken cancellationToken)
    {
        var baseSlug = Slugify(name);
        var slug = baseSlug;
        var suffix = 2;

        while (await _boards.GetBySlugAsync(slug, cancellationToken) is not null)
        {
            slug = $"{baseSlug}-{suffix}";
            suffix++;
        }

        return slug;
    }

    private static BoardResponse ToResponse(Board board)
    {
        return new BoardResponse(board.Id, board.Name, board.Slug, board.Description, board.IsPublic, board.CreatedAtUtc);
    }

    private static string Slugify(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        var builder = new StringBuilder(normalized.Length);
        var previousDash = false;

        foreach (var character in normalized)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                previousDash = false;
                continue;
            }

            if (!previousDash)
            {
                builder.Append('-');
                previousDash = true;
            }
        }

        var slug = builder.ToString().Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? "board" : slug;
    }
}
