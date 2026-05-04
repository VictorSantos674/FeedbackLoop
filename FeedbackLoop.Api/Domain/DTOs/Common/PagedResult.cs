namespace FeedbackLoop.Api.Domain.DTOs.Common;

public sealed record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
