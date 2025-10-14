using Backend.Application.Contracts.Users;

public interface IRoleBasisRepo
{
    /// <summary>
    /// 取得所有角色
    /// </summary>
    Task<List<RoleBasisDto>> GetAllRolesAsync(CancellationToken ct);

    /// <summary>
    /// 依角色代碼取得角色
    /// </summary>
    Task<RoleBasisDto?> GetRoleAsync(string roleId, CancellationToken ct);
}
