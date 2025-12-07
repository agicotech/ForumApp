using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ForumApp.Data;
using ForumApp.Models;
using System.Security.Claims;

namespace ForumApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TopicsController : ControllerBase
{
    private readonly ForumContext _context;

    public TopicsController(ForumContext context)
    {
        _context = context;
    }

    // GET: api/topics
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search)
    {
        var query = _context.Topics
            .Include(t => t.Author)
            .Include(t => t.Messages)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(t => t.Title.Contains(search) || 
                                    (t.Description != null && t.Description.Contains(search)));
        }

        var topics = await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.Description,
                t.CreatedAt,
                Author = new { t.Author.Id, t.Author.Username },
                MessageCount = t.Messages.Count
            })
            .ToListAsync();

        return Ok(topics);
    }

    // GET: api/topics/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var topic = await _context.Topics
            .Include(t => t.Author)
            .Include(t => t.Messages)
            .Where(t => t.Id == id)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.Description,
                t.CreatedAt,
                Author = new { t.Author.Id, t.Author.Username },
                MessageCount = t.Messages.Count
            })
            .FirstOrDefaultAsync();

        if (topic == null)
            return NotFound(new { message = "Тема не найдена" });

        return Ok(topic);
    }

    // POST: api/topics
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTopicRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            return Unauthorized();

        var topic = new Topic
        {
            Title = request.Title,
            Description = request.Description,
            AuthorId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Topics.Add(topic);
        await _context.SaveChangesAsync();

        var createdTopic = await _context.Topics
            .Include(t => t.Author)
            .Where(t => t.Id == topic.Id)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.Description,
                t.CreatedAt,
                Author = new { t.Author.Id, t.Author.Username }
            })
            .FirstOrDefaultAsync();

        return CreatedAtAction(nameof(GetById), new { id = topic.Id }, createdTopic);
    }

    // DELETE: api/topics/{id}
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var topic = await _context.Topics.FindAsync(id);

        if (topic == null)
            return NotFound(new { message = "Тема не найдена" });

        _context.Topics.Remove(topic);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Тема успешно удалена" });
    }
}

public record CreateTopicRequest(string Title, string? Description);
