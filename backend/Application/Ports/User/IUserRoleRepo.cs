// Application/Ports/IUserRoleRepo.cs
using System.Data;
using Backend.Application.Contracts.User;

namespace Backend.Application.Ports;

public interface IUserRoleRepo
{
    /// <summary>
    /// 取得指定使用者的所有角色
    /// </summary>
    Task<List<UserRoleDto>> GetUserRolesAsync(IDbConnection conn, IDbTransaction? tx, Guid userId, CancellationToken ct);

    /// <summary>
    /// 新增或更新使用者角色（若存在則更新 expireDate）
    /// </summary>
    Task AddUserRoleAsync(IDbConnection conn, IDbTransaction? tx, Guid userId, string roleId, DateTime? expireDate, CancellationToken ct);

    /// <summary>
    /// 移除使用者指定角色
    /// </summary>
    Task RemoveUserRoleAsync(IDbConnection conn, IDbTransaction? tx, Guid userId, string roleId, CancellationToken ct);
}
