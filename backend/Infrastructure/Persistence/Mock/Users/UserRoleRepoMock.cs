using Backend.Application.Contracts.User;
using Backend.Application.Ports;
using System.Collections.Concurrent;
using System.Data;
using System.Threading;

namespace Backend.Infrastructure.Persistence.Mock.Users
{
	public sealed class UserRoleRepoMock : IUserRoleRepo
	{
		private static readonly ConcurrentDictionary<Guid, List<UserRoleDto>> _userRoles = new();

		public UserRoleRepoMock()
		{
			// 建立假資料
			var user = UserRepoMock.SeedUsers.First();
			_userRoles[user.UserId] = new List<UserRoleDto>
			{
				new UserRoleDto
				{
					UserId = user.UserId,
					RoleId = "BOSS",
					CreateDate = DateTime.UtcNow
				}
			};
		}

		public Task<List<UserRoleDto>> GetUserRolesAsync(IDbConnection conn, IDbTransaction? tx, Guid userId, CancellationToken ct)
		{
			_userRoles.TryGetValue(userId, out var list);
			return Task.FromResult(list ?? new List<UserRoleDto>());
		}

		public Task AddUserRoleAsync(IDbConnection conn, IDbTransaction? tx, Guid userId, string roleId, DateTime? expireDate, CancellationToken ct)
		{
			var list = _userRoles.GetOrAdd(userId, _ => new List<UserRoleDto>());
			list.Add(new UserRoleDto { UserId = userId, RoleId = roleId, CreateDate = DateTime.UtcNow, ExpireDate = expireDate });
			return Task.CompletedTask;
		}

		public Task RemoveUserRoleAsync(IDbConnection conn, IDbTransaction? tx, Guid userId, string roleId, CancellationToken ct)
		{
			if (_userRoles.TryGetValue(userId, out var list))
				list.RemoveAll(x => x.RoleId == roleId);
			return Task.CompletedTask;
		}

		public Task<List<UserRoleWithNameDto>> GetUserRolesWithNameAsync(IDbConnection conn, IDbTransaction? tx, Guid userId, CancellationToken ct)
		{
			var result = new List<UserRoleWithNameDto>();
			if (_userRoles.TryGetValue(userId, out var list))
			{
				result = list.Select(r => new UserRoleWithNameDto
				{
					UserId = r.UserId,
					RoleId = r.RoleId,
					RoleName = RoleBasisRepoMock.TryGetRoleName(r.RoleId) ?? r.RoleId
				}).ToList();
			}
			return Task.FromResult(result);
		}
	}
}
