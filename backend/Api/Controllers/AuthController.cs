// Backend.Api/Controllers/AuthController.cs
using Backend.Contracts.User;
using Backend.Application.Services.Users;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Google 自動登入／註冊
    /// </summary>
    [HttpPost("google/auto")]
    public async Task<ActionResult<EnsureUserResponse>> EnsureGoogleAuto(
        [FromBody] GooglePayload payload,
        CancellationToken ct)
    {
        try
        {
            var (userDto, isNew) = await _authService.EnsureUserForGoogleAutoAsync(
                payload.Email,
                payload.DisplayName,
                payload.AvatarUrl,
                ct);

            return Ok(new EnsureUserResponse(userDto, isNew));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🔥 AutoEnsure failed: {ex.Message}");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    public record GooglePayload(string Email, string? DisplayName, string? AvatarUrl, AuthMode Mode);
}
