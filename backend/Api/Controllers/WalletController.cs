// Backend.Api/Controllers/WalletController.cs
using Backend.Application.Ports;
using Backend.Application.Wallet;
using Backend.Contracts.Wallet;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class WalletController : ControllerBase
{
    private readonly WalletService _svc;
    public WalletController(WalletService svc) => _svc = svc;
	private readonly IWalletQueryRepo _q;
	public WalletController(IWalletQueryRepo q) => _q = q;

	/// <summary>
	/// 新增一筆錢包流水帳（DB 觸發器會同步更新餘額與 BalanceAfter）
	/// </summary>
	[HttpPost("ledger")]
    public async Task<ActionResult<LedgerDto>> CreateLedger([FromBody] CreateLedgerRequest req, CancellationToken ct)
    {
        try
        {
            var dto = await _svc.CreateLedgerAsync(req, ct);
            return CreatedAtAction(nameof(GetLedger), new { id = dto.TxId }, dto);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Insufficient"))
        {
            return Conflict(new { message = "Insufficient balance." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message, param = ex.ParamName });
        }
    }

    // 方便 CreatedAtAction 對應；實作可改為查詢 DB
    [HttpGet("ledger/{id:long}")]
    public ActionResult GetLedger(long id) => Ok(new { txId = id });

	[HttpGet("balances")]
	public async Task<ActionResult<IEnumerable<BalanceDto>>> GetBalances([FromQuery] string currencies, CancellationToken ct)
	{
		// 從登入的 Claims 取 userId（依你登入邏輯調整）
		var uidText = User.FindFirstValue(ClaimTypes.NameIdentifier)
					  ?? User.FindFirstValue("sub"); // or your custom claim
		if (!Guid.TryParse(uidText, out var userId))
			return Unauthorized();

		var list = currencies?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
				   ?? Array.Empty<string>();

		if (list.Length == 0)
			list = new[] { "Gold", "Silver" }; // 預設

		var data = await _q.GetBalancesAsync(userId, list, ct);
		return Ok(data);
	}
}
