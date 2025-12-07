using ForumApp.Data;
using ForumApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ForumApp.Services;

public class AuditService
{
    private readonly ForumContext _context;

    public AuditService(ForumContext context)
    {
        _context = context;
    }

    public async Task LogActionAsync(int userId, string action, string? entityType = null, int? entityId = null, string? details = null)
    {
        var auditLog = new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Timestamp = DateTime.UtcNow,
            Details = details
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }

    public async Task<List<AuditLog>> GetAllLogsAsync()
    {
        return await _context.AuditLogs
            .Include(a => a.User)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task<List<AuditLog>> GetLogsByUserAsync(int userId)
    {
        return await _context.AuditLogs
            .Include(a => a.User)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }
}
