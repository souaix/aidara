using Backend.Contracts.Users;
using Backend.Application.Services.Users;
using Backend.Infrastructure.Persistence.Postgres;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly NpgsqlDataSource _ds;
    private readonly AuthService _authService;

    public AuthController(NpgsqlDataSource ds, AuthService authService)
    {
        _ds = ds;
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
        await using var uow = new _UnitOfWork(_ds);
        var db = await uow.BeginAsync(ct);

        try
        {
            var (userDto, isNew) = await _authService.EnsureUserForGoogleAutoAsync(
                db,
                payload.Email,
                payload.DisplayName,
                payload.AvatarUrl,
                ct);

            await uow.CommitAsync(ct);
            return Ok(new EnsureUserResponse(userDto, isNew));
        }
        catch (Exception ex)
        {
            await uow.RollbackAsync();
            Console.WriteLine($"🔥 AutoEnsure failed: {ex.Message}");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    public record GooglePayload(string Email, string? DisplayName, string? AvatarUrl, AuthMode Mode);
}
