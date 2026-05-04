using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FeedbackLoop.Api.Domain.Common;

namespace FeedbackLoop.Api.Domain.Entities;

public class Vote : IWorkspaceScopedEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid WorkspaceId { get; set; }

    [Required]
    public Guid PostId { get; set; }

    [ForeignKey(nameof(PostId))]
    public Post Post { get; set; } = null!;

    [Required]
    public Guid EndUserToken { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
