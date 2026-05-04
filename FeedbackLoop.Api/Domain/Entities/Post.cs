using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FeedbackLoop.Api.Domain.Common;
using FeedbackLoop.Api.Domain.Enums;

namespace FeedbackLoop.Api.Domain.Entities;

public class Post : IWorkspaceScopedEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid WorkspaceId { get; set; }

    [Required]
    public Guid BoardId { get; set; }

    [ForeignKey(nameof(BoardId))]
    public Board Board { get; set; } = null!;

    [Required]
    [MaxLength(180)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(4000)]
    public string Description { get; set; } = string.Empty;

    public PostStatus Status { get; set; } = PostStatus.Open;

    [Required]
    public Guid EndUserToken { get; set; }

    [MaxLength(160)]
    public string? EndUserName { get; set; }

    [MaxLength(254)]
    public string? EndUserEmail { get; set; }

    public int VoteCount { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<Vote> Votes { get; set; } = new List<Vote>();

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public ICollection<StatusHistory> StatusHistory { get; set; } = new List<StatusHistory>();
}
