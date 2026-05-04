using System.ComponentModel.DataAnnotations;

namespace FeedbackLoop.Api.Domain.Entities;

public class Workspace
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(160)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string Slug { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();

    public ICollection<Board> Boards { get; set; } = new List<Board>();
}
