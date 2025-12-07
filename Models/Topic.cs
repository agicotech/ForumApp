using System.ComponentModel.DataAnnotations;

namespace ForumApp.Models;

public class Topic
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Название темы обязательно.")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "Название темы должно быть от 5 до 200 символов.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов.")]
    public string? Description { get; set; }

    [Required]
    public int AuthorId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User Author { get; set; } = null!;
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
