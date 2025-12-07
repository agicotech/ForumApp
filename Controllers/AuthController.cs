using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ForumApp.Services;
using System.Security.Claims;

namespace ForumApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly AuditService _auditService;

    public AuthController(AuthService authService, AuditService auditService)
    {
        _authService = authService;
        _auditService = auditService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _authService.RegisterAsync(request.Username, request.Email, request.Password);

        if (user == null)
            return BadRequest(new { message = "Пользователь с таким именем или email уже существует" });

        await _auditService.LogActionAsync(user.Id, "Register", "User", user.Id);

        return Ok(new
        {
            message = "Регистрация успешна",
            user = new
            {
                user.Id,
                user.Username,
                user.Email,
                user.Role
            }
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (user, token) = await _authService.LoginAsync(request.Username, request.Password);

        if (user == null || token == null)
            return Unauthorized(new { message = "Неверное имя пользователя или пароль" });

        await _auditService.LogActionAsync(user.Id, "Login");

        return Ok(new
        {
            message = "Вход выполнен успешно",
            token,
            user = new
            {
                id = user.Id,
                username = user.Username,
                email = user.Email,
                role = user.Role.ToString()
            }
        });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            return Unauthorized();

        var success = await _authService.ChangePasswordAsync(userId, request.OldPassword, request.NewPassword);

        if (!success)
            return BadRequest(new { message = "Неверный старый пароль" });

        await _auditService.LogActionAsync(userId, "ChangePassword");

        return Ok(new { message = "Пароль успешно изменен" });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("promote-to-admin/{userId}")]
    public async Task<IActionResult> PromoteToAdmin(int userId)
    {
        var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (adminIdClaim == null || !int.TryParse(adminIdClaim.Value, out int adminId))
            return Unauthorized();

        var success = await _authService.PromoteToAdminAsync(userId);

        if (!success)
            return BadRequest(new { message = "Не удалось повысить пользователя до администратора" });

        await _auditService.LogActionAsync(adminId, "PromoteToAdmin", "User", userId, $"User {userId} promoted to Admin");

        return Ok(new { message = "Пользователь успешно повышен до администратора" });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _authService.GetAllUsersAsync();
        return Ok(users.Select(u => new
        {
            id = u.Id,
            username = u.Username,
            email = u.Email,
            role = u.Role.ToString(),
            createdAt = u.CreatedAt
        }));
    }
}

public record RegisterRequest(string Username, string Email, string Password);
public record LoginRequest(string Username, string Password);
public record ChangePasswordRequest(string OldPassword, string NewPassword);
