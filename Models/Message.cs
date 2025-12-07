using System.ComponentModel.DataAnnotations;

namespace ForumApp.Models;

public class Message
{
    public int Id { get; set; }

    [Required]
    public int TopicId { get; set; }

    [Required]
    public int AuthorId { get; set; }

    [Required(ErrorMessage = "Текст сообщения обязателен.")]
    [StringLength(5000, MinimumLength = 1, ErrorMessage = "Текст сообщения должен быть от 1 до 5000 символов.")]
    public string Text { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Topic Topic { get; set; } = null!;
    public User Author { get; set; } = null!;
}
