using Backend.Application.Ports;
using System.Collections.Concurrent;
using System.Data;
using System.Threading;

namespace Backend.Infrastructure.Persistence.Mock.Users
{
	public sealed class UserActiveModeRepoMock : IUserActiveModeRepo
	{
		private static readonly ConcurrentDictionary<Guid, string> _activeRole = new();

		public UserActiveModeRepoMock()
		{
			// 預設測試使用者啟用 "BOSS" 模式
			var user = UserRepoMock.SeedUsers.First();
			_activeRole[user.UserId] = "BOSS";
		}

		public Task<string?> GetActiveModeAsync(IDbConnection conn, IDbTransaction? tx, Guid userId, CancellationToken ct)
		{
			_activeRole.TryGetValue(userId, out var role);
			return Task.FromResult(role);
		}

		public Task SetActiveModeAsync(IDbConnection conn, IDbTransaction? tx, Guid userId, string roleId, CancellationToken ct)
		{
			_activeRole[userId] = roleId;
			return Task.CompletedTask;
		}
	}
}
