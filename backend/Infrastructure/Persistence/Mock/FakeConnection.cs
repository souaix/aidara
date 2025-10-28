// Infrastructure/Persistence/Mock/FakeConnection.cs
using System.Data;

namespace Backend.Infrastructure.Persistence.Mock
{
	internal sealed class FakeConnection : IDbConnection
	{
		public string ConnectionString { get; set; } = "FakeConnection";
		public int ConnectionTimeout => 0;
		public string Database => "MockDB";
		public ConnectionState State => ConnectionState.Open;

		public IDbTransaction BeginTransaction() => new FakeTransaction();
		public IDbTransaction BeginTransaction(IsolationLevel il) => new FakeTransaction();
		public void ChangeDatabase(string databaseName) { }
		public void Close() { }
		public IDbCommand CreateCommand() => throw new NotSupportedException("FakeConnection doesn't execute SQL.");
		public void Open() { }
		public void Dispose() { }

		private sealed class FakeTransaction : IDbTransaction
		{
			public IDbConnection Connection => null!;
			public IsolationLevel IsolationLevel => IsolationLevel.ReadCommitted;
			public void Commit() { }
			public void Rollback() { }
			public void Dispose() { }
		}
	}
}
