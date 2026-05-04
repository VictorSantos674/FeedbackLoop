using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FeedbackLoop.Api.Domain.Common;

namespace FeedbackLoop.Api.Domain.Entities;

public class Comment : IWorkspaceScopedEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid WorkspaceId { get; set; }

    [Required]
    public Guid PostId { get; set; }

    [ForeignKey(nameof(PostId))]
    public Post Post { get; set; } = null!;

    public Guid? AuthorUserId { get; set; }

    [ForeignKey(nameof(AuthorUserId))]
    public User? AuthorUser { get; set; }

    public Guid? EndUserToken { get; set; }

    [Required]
    [MaxLength(3000)]
    public string Body { get; set; } = string.Empty;

    public bool IsTeamReply { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
