using FeedbackLoop.Api.Domain.DTOs.Boards;

namespace FeedbackLoop.Api.Application.Services;

public interface IBoardService
{
    Task<BoardResponse> CreateAsync(CreateBoardRequest request, Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BoardSummaryResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<BoardDetailResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<BoardDetailResponse> UpdateAsync(Guid id, UpdateBoardRequest request, Guid userId, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
}
