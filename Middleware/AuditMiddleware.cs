using System.Security.Claims;
using ForumApp.Services;

namespace ForumApp.Middleware;

public class AuditMiddleware
{
    private readonly RequestDelegate _next;

    public AuditMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AuditService auditService)
    {
        await _next(context);

        // Log only authenticated users' actions
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                var method = context.Request.Method;
                var path = context.Request.Path.Value ?? "";

                // Log only modifying operations
                if (method == "POST" || method == "PUT" || method == "DELETE")
                {
                    var action = $"{method} {path}";
                    await auditService.LogActionAsync(userId, action, details: $"Status: {context.Response.StatusCode}");
                }
            }
        }
    }
}
