// Backend.Infrastructure.Persistence.Postgres/WalletRepo.cs
using Backend.Application.Ports;
using Backend.Contracts.Wallet;
using Backend.Domain.Wallet;
using Dapper;
using Npgsql;
namespace Backend.Infrastructure.Persistence.Postgres;

public sealed class WalletRepo : IWalletRepo
{
    private readonly NpgsqlConnection _conn;
    private readonly NpgsqlTransaction? _tx;

    public WalletRepo(NpgsqlConnection conn, NpgsqlTransaction? tx)
    {
        _conn = conn;
        _tx = tx;
    }

    public async Task<long> GetCurrentBalanceAsync(Guid userId, string currency, CancellationToken ct)
    {
        const string sql = """
            select "BALANCE"
            from "WALLET_BALANCES"
            where "USER_ID" = @userId and "CURRENCY" = @currency
            """;
        var bal = await _conn.QueryFirstOrDefaultAsync<long?>(
            new CommandDefinition(sql, new { userId, currency }, _tx, cancellationToken: ct));
        return bal ?? 0;
    }

    public async Task<LedgerDto> InsertAsync(CreateLedgerRequest req, CancellationToken ct)
    {
        const string sql = """
    insert into "WALLET_LEDGER"
        ("USER_ID","ORDER_ID","TX_TYPE","AMOUNT","CURRENCY","META")
    values
        (@UserId, @OrderId, @TxType::"TX_TYPE", @Amount, @Currency, to_jsonb(@Meta))
    returning
        "TX_ID"          as TxId,
        "USER_ID"        as UserId,
        "ORDER_ID"       as OrderId,
        "TX_TYPE"::text  as TxType,  -- 👈 強制轉 text，避免 enum handler
        "AMOUNT"         as Amount,
        "CURRENCY"       as Currency,
        "BALANCE_AFTER"  as BalanceAfter,
        "META"::text     as Meta,
        "CREATED_AT"     as CreatedAt;
    """;

        return await _conn.QuerySingleAsync<LedgerDto>(
            new CommandDefinition(sql, new
            {
                req.UserId,
                req.OrderId,
                req.TxType,     // 👈 直接 string
                req.Amount,
                req.Currency,
                req.Meta
            }, _tx, cancellationToken: ct));


    }
}
