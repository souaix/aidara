using Backend.Contracts.Wallet;

namespace Backend.Application.Ports;

public interface IWalletQueryRepo
{
	/// <summary>
	/// 取得指定使用者在多個幣別的餘額（沒有的幣別不會回傳）。
	/// </summary>
	Task<IEnumerable<BalanceDto>> GetBalancesAsync(
		Guid userId,
		string[] currencies,
		CancellationToken ct);
}
