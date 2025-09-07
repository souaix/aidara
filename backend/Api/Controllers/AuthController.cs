using Backend.Application.Users;
using Backend.Contracts.Users;
using Backend.Application.Ports;   // 注意：要引用 Ports 取得 IUserRepo
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;
    private readonly Func<IUserRepo> _userRepoFactory;

    public AuthController(AuthService auth, Func<IUserRepo> userRepoFactory)
    {
        _auth = auth;
        _userRepoFactory = userRepoFactory;
    }

    [HttpPost("google/ensure")]
    public async Task<ActionResult<UserDto>> EnsureGoogle([FromBody] GooglePayload p, CancellationToken ct)
    {
        var dto = await _auth.EnsureUserForGoogleAsync(
            p.Email, p.DisplayName, p.AvatarUrl, p.Mode, _userRepoFactory, ct);

        if (dto is null) return NotFound(new { message = "User not found in login mode." });
        return Ok(dto);
    }

    public record GooglePayload(string Email, string? DisplayName, string? AvatarUrl, AuthMode Mode);
}
