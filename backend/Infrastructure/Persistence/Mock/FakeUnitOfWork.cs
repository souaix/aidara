using Backend.Application.Shared;
using Backend.Infrastructure.Persistence.Mock;
using System.Data;

public sealed class FakeUnitOfWork : IUnitOfWork
{
	private readonly IDbConnection _conn = new FakeConnection();

	public IDbConnection Connection => _conn;
	public IDbTransaction? Transaction => null;

	public Task CommitAsync(CancellationToken ct = default) => Task.CompletedTask;
	public Task RollbackAsync(CancellationToken ct = default) => Task.CompletedTask;
	public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
