using Backend.Application.Ports;
using Backend.Domain.Entities;
using System.Collections.Concurrent;
using System.Data;
using System.Threading;

namespace Backend.Infrastructure.Persistence.Mock.Users
{
	public sealed class UserRepoMock : IUserRepo
	{
		private static readonly ConcurrentDictionary<Guid, User> _users = new();
		private static readonly ConcurrentDictionary<string, Guid> _emailToId = new(StringComparer.OrdinalIgnoreCase);

		public UserRepoMock()
		{
			// 建立假使用者  
			var user = new User
			{
				UserId = Guid.Parse("4d0c960f-5fda-4605-9c5a-ca087fe7f714"),
				Email = "boss@example.com",
				DisplayName = "測試小老闆"
			};
			_users[user.UserId] = user;
			_emailToId[user.Email] = user.UserId;
		}

		public Task<User?> GetByEmailAsync(IDbConnection conn, IDbTransaction? tx, string email, CancellationToken ct)
		{
			if (_emailToId.TryGetValue(email, out var id) && _users.TryGetValue(id, out var user))
				return Task.FromResult<User?>(user);
			return Task.FromResult<User?>(null);
		}

		public Task<User> InsertAsync(IDbConnection conn, IDbTransaction? tx, User user, CancellationToken ct)
		{
			var id = user.UserId == Guid.Empty ? Guid.NewGuid() : user.UserId;
			user.UserId = id;
			_users[id] = user;
			if (!string.IsNullOrWhiteSpace(user.Email))
				_emailToId[user.Email] = id;
			return Task.FromResult(user);
		}

		public Task<User?> GetUserProfileAsync(IDbConnection conn, IDbTransaction? tx, Guid userId, CancellationToken ct)
		{
			_users.TryGetValue(userId, out var user);
			return Task.FromResult<User?>(user);
		}

		public Task TouchLastSeenAsync(IDbConnection conn, IDbTransaction? tx, Guid userId, CancellationToken ct)
		{
			if (_users.TryGetValue(userId, out var user))
			{
				var prop = typeof(User).GetProperty("LastSeenAt");
				prop?.SetValue(user, DateTime.UtcNow);
			}
			return Task.CompletedTask;
		}

		public static IEnumerable<User> SeedUsers => _users.Values;
	}
}
