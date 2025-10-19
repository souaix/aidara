// Application/Ports/IRoleBasisRepo.cs
using System.Data;
using Backend.Application.Contracts.User;

namespace Backend.Application.Ports;

public interface IRoleBasisRepo
{
    /// <summary>
    /// 取得所有角色
    /// </summary>
    Task<List<RoleBasisDto>> GetAllRolesAsync(IDbConnection conn, IDbTransaction? tx, CancellationToken ct);

    /// <summary>
    /// 依角色代碼取得角色
    /// </summary>
    Task<RoleBasisDto?> GetRoleAsync(IDbConnection conn, IDbTransaction? tx, string roleId, CancellationToken ct);
}
