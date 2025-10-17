using System.Data;
using Npgsql;
using Backend.Application.Shared;
namespace Infrastructure.Persistence.Shared
{
	/// <summary>
	/// PostgreSQL 專用的 UnitOfWork，負責連線與交易管理。
	/// </summary>
	public sealed class PostgreSqlUnitOfWork : IUnitOfWork
	{
		private readonly NpgsqlConnection _conn;
		private readonly NpgsqlTransaction? _tx;
		private bool _completed;

		/// <summary>
		/// 建立新的 PostgreSQL UnitOfWork。
		/// </summary>
		/// <param name="connectionString">資料庫連線字串</param>
		/// <param name="withTransaction">是否啟用交易（預設 true）</param>
		public PostgreSqlUnitOfWork(string connectionString, bool withTransaction = true)
		{
			_conn = new NpgsqlConnection(connectionString);
			_conn.Open();

			if (withTransaction)
			{
				_tx = _conn.BeginTransaction();
			}
		}

		public IDbConnection Connection => _conn;
		public IDbTransaction? Transaction => _tx;

		public Task CommitAsync(CancellationToken ct = default)
		{
			_tx?.Commit();
			_completed = true;
			return Task.CompletedTask;
		}

		public Task RollbackAsync(CancellationToken ct = default)
		{
			if (!_completed && _tx != null)
			{
				_tx.Rollback();
			}
			return Task.CompletedTask;
		}

		public async ValueTask DisposeAsync()
		{
			if (!_completed && _tx != null)
			{
				try { _tx.Rollback(); } catch { /* ignore */ }
			}

			_tx?.Dispose();
			await _conn.DisposeAsync();
		}
	}

	/// <summary>
	/// PostgreSQL UnitOfWork 的 Factory，負責建立新的交易範圍。
	/// </summary>
	public sealed class PostgreSqlUnitOfWorkFactory : IUnitOfWorkFactory
	{
		private readonly string _connectionString;

		public PostgreSqlUnitOfWorkFactory(string connectionString)
		{
			_connectionString = connectionString;
		}

		public Task<IUnitOfWork> BeginAsync(bool withTransaction = true, CancellationToken ct = default)
		{
			IUnitOfWork uow = new PostgreSqlUnitOfWork(_connectionString, withTransaction);
			return Task.FromResult(uow);
		}
	}
}
