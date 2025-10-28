using Backend.Application.Contracts.User;
using Backend.Application.Ports;
using System.Collections.Concurrent;
using System.Data;
using System.Threading;

namespace Backend.Infrastructure.Persistence.Mock.Users
{
	public sealed class RoleBasisRepoMock : IRoleBasisRepo
	{
		private static readonly ConcurrentDictionary<string, string> _roles = new(StringComparer.OrdinalIgnoreCase);

		public RoleBasisRepoMock()
		{
			// 建立假資料
			_roles["ADMIN"] = "管理者";
			_roles["BOSS"] = "小老闆";
			_roles["CUSTOMER"] = "顧客";
		}

		public Task<List<RoleBasisDto>> GetAllRolesAsync(IDbConnection conn, IDbTransaction? tx, CancellationToken ct)
		{
			var list = _roles.Select(kv => new RoleBasisDto { RoleId = kv.Key, RoleName = kv.Value }).ToList();
			return Task.FromResult(list);
		}

		public Task<RoleBasisDto?> GetRoleAsync(IDbConnection conn, IDbTransaction? tx, string roleId, CancellationToken ct)
		{
			_roles.TryGetValue(roleId, out var name);
			return Task.FromResult<RoleBasisDto?>(name == null ? null : new RoleBasisDto { RoleId = roleId, RoleName = name });
		}

		internal static string? TryGetRoleName(string roleId) =>
			_roles.TryGetValue(roleId, out var name) ? name : null;
	}
}
