using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FeedbackLoop.Api.Domain.Common;
using FeedbackLoop.Api.Domain.Enums;

namespace FeedbackLoop.Api.Domain.Entities;

public class StatusHistory : IWorkspaceScopedEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid WorkspaceId { get; set; }

    [Required]
    public Guid PostId { get; set; }

    [ForeignKey(nameof(PostId))]
    public Post Post { get; set; } = null!;

    public PostStatus? FromStatus { get; set; }

    public PostStatus ToStatus { get; set; }

    [Required]
    public Guid ChangedByUserId { get; set; }

    [ForeignKey(nameof(ChangedByUserId))]
    public User ChangedByUser { get; set; } = null!;

    public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;
}
