using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ForumApp.Services;

namespace ForumApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AuditController : ControllerBase
{
    private readonly AuditService _auditService;

    public AuditController(AuditService auditService)
    {
        _auditService = auditService;
    }

    // GET: api/audit
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var logs = await _auditService.GetAllLogsAsync();

        var result = logs.Select(log => new
        {
            log.Id,
            log.UserId,
            Username = log.User.Username,
            log.Action,
            log.EntityType,
            log.EntityId,
            log.Timestamp,
            log.Details
        });

        return Ok(result);
    }

    // GET: api/audit/user/{userId}
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(int userId)
    {
        var logs = await _auditService.GetLogsByUserAsync(userId);

        var result = logs.Select(log => new
        {
            log.Id,
            log.UserId,
            Username = log.User.Username,
            log.Action,
            log.EntityType,
            log.EntityId,
            log.Timestamp,
            log.Details
        });

        return Ok(result);
    }
}
