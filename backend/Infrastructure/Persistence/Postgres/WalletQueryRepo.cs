// Backend.Infrastructure.Persistence.Postgres/WalletQueryRepo.cs
using Backend.Application.Ports;
using Backend.Contracts.Wallet;
using Dapper;
using Npgsql;

public sealed class WalletQueryRepo : IWalletQueryRepo
{
	private readonly NpgsqlDataSource _ds;
	public WalletQueryRepo(NpgsqlDataSource ds) => _ds = ds;

	public async Task<IEnumerable<BalanceDto>> GetBalancesAsync(Guid userId, string[] currencies, CancellationToken ct)
	{
		const string sql = """
            select "CURRENCY" as "Currency", "BALANCE" as "Balance"
            from "WALLET_BALANCES"
            where "USER_ID" = @userId
              and "CURRENCY" = any(@currs)
            """;
		await using var conn = await _ds.OpenConnectionAsync(ct);
		return await conn.QueryAsync<BalanceDto>(
			new CommandDefinition(sql, new { userId, currs = currencies }, cancellationToken: ct));
	}
}
