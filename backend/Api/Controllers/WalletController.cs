// Backend.Api/Controllers/WalletController.cs
using Backend.Application.Wallet;
using Backend.Contracts.Wallet;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class WalletController : ControllerBase
{
    private readonly WalletService _svc;
    public WalletController(WalletService svc) => _svc = svc;

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
}
