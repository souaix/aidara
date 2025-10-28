// Infrastructure/Persistence/Mock/FakeUnitOfWorkFactory.cs
using Backend.Application.Shared;

namespace Backend.Infrastructure.Persistence.Mock
{
	/// <summary>
	/// 回傳 FakeUnitOfWork 的工廠，避免建立實際 PostgreSQL 連線。
	/// </summary>
	public sealed class FakeUnitOfWorkFactory : IUnitOfWorkFactory
	{
		public Task<IUnitOfWork> BeginAsync(bool withTransaction = true, CancellationToken ct = default)
			=> Task.FromResult<IUnitOfWork>(new FakeUnitOfWork());
	}
}
