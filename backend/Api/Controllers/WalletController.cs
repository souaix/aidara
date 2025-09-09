// Backend.Api.Controllers.WalletController.cs

using Backend.Application.Ports;
using Backend.Contracts.Wallet;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/wallet")]
public class WalletController : ControllerBase
{
	private readonly IWalletQueryRepo _q;

	public WalletController(IWalletQueryRepo q)
	{
		_q = q;
	}

	[HttpGet("balances")]
	public async Task<ActionResult<IEnumerable<BalanceDto>>> GetBalances(
		[FromQuery] Guid userId,
		[FromQuery] string[] currencies,
		CancellationToken ct)
	{
		if (currencies is null || currencies.Length == 0)
			currencies = new[] { "Gold", "Silver" };

		var data = await _q.GetBalancesAsync(userId, currencies, ct);
		return Ok(data);
	}
}
