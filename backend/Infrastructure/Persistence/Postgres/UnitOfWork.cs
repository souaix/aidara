using Backend.Application.Ports;
using Backend.Infrastructure.Persistence.Postgres;
using Npgsql;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly NpgsqlDataSource _ds;
    private NpgsqlConnection? _conn;
    private NpgsqlTransaction? _tx;

    public UnitOfWork(NpgsqlDataSource ds) => _ds = ds;

    public async Task BeginAsync(CancellationToken ct)
    {
        _conn = (NpgsqlConnection)await _ds.OpenConnectionAsync(ct);
        _tx = await _conn.BeginTransactionAsync(ct);
    }

    public Task CommitAsync(CancellationToken ct) => _tx!.CommitAsync(ct);
    public Task RollbackAsync() => _tx!.RollbackAsync();

    public async ValueTask DisposeAsync()
    {
        await (_tx?.DisposeAsync() ?? ValueTask.CompletedTask);
        await (_conn?.DisposeAsync() ?? ValueTask.CompletedTask);
    }

    public IUserRepo CreateUserRepo() => new UserRepo(_conn!, _tx);

    public IWalletRepo CreateWalletRepo() => new WalletRepo(_conn!, _tx);

    public IBossServiceRepo CreateBossServiceRepo() => new BossServiceRepo(_conn!, _tx);

}
