using System.ComponentModel.DataAnnotations;

namespace ForumApp.Models;

public class AuditLog
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    [StringLength(100)]
    public string Action { get; set; } = string.Empty;

    [StringLength(50)]
    public string? EntityType { get; set; }

    public int? EntityId { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [StringLength(500)]
    public string? Details { get; set; }

    // Navigation property
    public User User { get; set; } = null!;
}
