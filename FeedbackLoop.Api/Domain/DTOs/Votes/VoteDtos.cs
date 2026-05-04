using System.ComponentModel.DataAnnotations;

namespace FeedbackLoop.Api.Domain.DTOs.Votes;

public sealed record ToggleVoteRequest([property: Required] string EndUserToken);

public sealed record VoteResult(bool Voted, int VoteCount);
