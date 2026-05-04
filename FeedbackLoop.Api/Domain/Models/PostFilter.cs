using FeedbackLoop.Api.Domain.Enums;

namespace FeedbackLoop.Api.Domain.Models;

public sealed record PostFilter(
    PostStatus? Status,
    string? Search,
    PostSortBy SortBy = PostSortBy.MostVoted,
    int Page = 1,
    int PageSize = 20,
    string? EndUserToken = null);

public enum PostSortBy
{
    MostVoted,
    Newest,
    Oldest,
    RecentlyUpdated
}
