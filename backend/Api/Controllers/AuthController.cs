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

	[HttpPost("google/auto")]
	public async Task<ActionResult<UserDto>> EnsureGoogleAuto([FromBody] GooglePayload p, CancellationToken ct)
	{
		try
		{
			var dto = await _auth.EnsureUserForGoogleAutoAsync(
				p.Email, p.DisplayName, p.AvatarUrl, _userRepoFactory, ct);

			return Ok(dto);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"🔥 AutoEnsure failed: {ex.Message}");
			return StatusCode(500, new { message = ex.Message });
		}
	}

	public record GooglePayload(string Email, string? DisplayName, string? AvatarUrl, AuthMode Mode);
}
