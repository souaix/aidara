using Backend.Application.Contracts.Users;

public interface IRoleBasisRepo
{
    /// <summary>
    /// 取得所有角色
    /// </summary>
    Task<List<RoleBasisDto>> GetAllRolesAsync(IUowContext uow, CancellationToken ct);

    /// <summary>
    /// 依角色代碼取得角色
    /// </summary>
    Task<RoleBasisDto?> GetRoleAsync(IUowContext uow, string roleId, CancellationToken ct);
}
