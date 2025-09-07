namespace Backend.Application.Ports;

public interface IUnitOfWork : IAsyncDisposable
{
    Task BeginAsync(CancellationToken ct);
    Task CommitAsync(CancellationToken ct);
    Task RollbackAsync();

    IWalletRepo CreateWalletRepo();
}
