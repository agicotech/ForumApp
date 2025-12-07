using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ForumApp.Models;

public class User
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Имя пользователя обязательно.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Имя пользователя должно быть от 3 до 50 символов.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email обязателен.")]
    [EmailAddress(ErrorMessage = "Некорректный формат email.")]
    public string Email { get; set; } = string.Empty;

    [JsonIgnore]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; } = UserRole.User;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Topic> Topics { get; set; } = new List<Topic>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}

public enum UserRole
{
    Guest = 0,
    User = 1,
    Admin = 2
}
