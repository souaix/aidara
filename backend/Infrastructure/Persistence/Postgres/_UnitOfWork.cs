// Infrastructure/Persistence/Postgres/_UnitOfWork.cs

using System.Data;
using Npgsql;

public sealed class _UnitOfWork : IUowContext
{
    private readonly NpgsqlDataSource _ds;
    private NpgsqlConnection? _conn;
    private NpgsqlTransaction? _tx;

    public _UnitOfWork(NpgsqlDataSource ds) => _ds = ds;

    public IDbConnection Connection => _conn
        ?? throw new InvalidOperationException("UoW not begun. Call BeginAsync first.");

    public IDbTransaction? Transaction => _tx;

    public async Task<IUowContext> BeginAsync(
        CancellationToken ct,
        IsolationLevel isolation = IsolationLevel.ReadCommitted)
    {
        _conn = (NpgsqlConnection)await _ds.OpenConnectionAsync(ct);
        _tx = await _conn.BeginTransactionAsync(isolation, ct);
        return this; // 讓你能 var db = await uow.BeginAsync(...)
    }

    public Task CommitAsync(CancellationToken ct)
        => _tx?.CommitAsync(ct) ?? Task.CompletedTask;

    public Task RollbackAsync()
        => _tx?.RollbackAsync() ?? Task.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        if (_tx != null) await _tx.DisposeAsync();
        if (_conn != null) await _conn.DisposeAsync();
        _tx = null; _conn = null;
    }
}
