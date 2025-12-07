using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ForumApp.Data;
using ForumApp.Models;
using System.Security.Claims;

namespace ForumApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly ForumContext _context;

    public MessagesController(ForumContext context)
    {
        _context = context;
    }

    // GET: api/messages/topic/{topicId}
    [HttpGet("topic/{topicId}")]
    public async Task<IActionResult> GetByTopic(int topicId, [FromQuery] string? search)
    {
        var query = _context.Messages
            .Include(m => m.Author)
            .Where(m => m.TopicId == topicId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(m => m.Text.Contains(search));
        }

        var messages = await query
            .OrderBy(m => m.CreatedAt)
            .Select(m => new
            {
                m.Id,
                m.Text,
                m.CreatedAt,
                m.UpdatedAt,
                Author = new { m.Author.Id, m.Author.Username }
            })
            .ToListAsync();

        return Ok(messages);
    }

    // GET: api/messages/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var message = await _context.Messages
            .Include(m => m.Author)
            .Where(m => m.Id == id)
            .Select(m => new
            {
                m.Id,
                m.TopicId,
                m.Text,
                m.CreatedAt,
                m.UpdatedAt,
                Author = new { m.Author.Id, m.Author.Username }
            })
            .FirstOrDefaultAsync();

        if (message == null)
            return NotFound(new { message = "Сообщение не найдено" });

        return Ok(message);
    }

    // POST: api/messages
    [Authorize(Roles = "User,Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMessageRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            return Unauthorized();

        // Check if topic exists
        var topicExists = await _context.Topics.AnyAsync(t => t.Id == request.TopicId);
        if (!topicExists)
            return BadRequest(new { message = "Тема не найдена" });

        var message = new Message
        {
            TopicId = request.TopicId,
            AuthorId = userId,
            Text = request.Text,
            CreatedAt = DateTime.UtcNow
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        var createdMessage = await _context.Messages
            .Include(m => m.Author)
            .Where(m => m.Id == message.Id)
            .Select(m => new
            {
                m.Id,
                m.TopicId,
                m.Text,
                m.CreatedAt,
                Author = new { m.Author.Id, m.Author.Username }
            })
            .FirstOrDefaultAsync();

        return CreatedAtAction(nameof(GetById), new { id = message.Id }, createdMessage);
    }

    // PUT: api/messages/{id}
    [Authorize(Roles = "User,Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMessageRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            return Unauthorized();

        var message = await _context.Messages.FindAsync(id);

        if (message == null)
            return NotFound(new { message = "Сообщение не найдено" });

        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        // Check if user is author or admin
        if (message.AuthorId != userId && userRole != "Admin")
            return Forbid();

        message.Text = request.Text;
        message.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var updatedMessage = await _context.Messages
            .Include(m => m.Author)
            .Where(m => m.Id == id)
            .Select(m => new
            {
                m.Id,
                m.TopicId,
                m.Text,
                m.CreatedAt,
                m.UpdatedAt,
                Author = new { m.Author.Id, m.Author.Username }
            })
            .FirstOrDefaultAsync();

        return Ok(updatedMessage);
    }

    // DELETE: api/messages/{id}
    [Authorize(Roles = "User,Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            return Unauthorized();

        var message = await _context.Messages.FindAsync(id);

        if (message == null)
            return NotFound(new { message = "Сообщение не найдено" });

        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        // Check if user is author or admin
        if (message.AuthorId != userId && userRole != "Admin")
            return Forbid();

        _context.Messages.Remove(message);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Сообщение успешно удалено" });
    }
}

public record CreateMessageRequest(int TopicId, string Text);
public record UpdateMessageRequest(string Text);
