using Backend.Application.Contracts.Users;

public interface IUserRoleRepo
{
    /// <summary>
    /// 取得指定使用者的所有角色
    /// </summary>
    Task<List<UserRoleDto>> GetUserRolesAsync(IUowContext uow, Guid userId, CancellationToken ct);

    /// <summary>
    /// 新增或更新使用者角色（若存在則更新 expireDate）
    /// </summary>
    Task AddUserRoleAsync(IUowContext uow, Guid userId, string roleId, DateTime? expireDate, CancellationToken ct);

    /// <summary>
    /// 移除使用者指定角色
    /// </summary>
    Task RemoveUserRoleAsync(IUowContext uow, Guid userId, string roleId, CancellationToken ct);
}
