// Application/Ports/IUowContext.cs
using System.Data;

public interface IUowContext : IAsyncDisposable
{
    IDbConnection Connection { get; }
    IDbTransaction? Transaction { get; }

    // 回傳自己，方便你寫 var db = await uow.BeginAsync(...)
    Task<IUowContext> BeginAsync(
        CancellationToken ct,
        IsolationLevel isolation = IsolationLevel.ReadCommitted);

    Task CommitAsync(CancellationToken ct);
    Task RollbackAsync();
}
