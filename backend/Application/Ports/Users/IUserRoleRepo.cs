using Backend.Application.ViewModels.Users;

public interface IUserRoleRepo
{
    /// <summary>
    /// 取得指定使用者的所有角色
    /// </summary>
    Task<List<UserRoleDto>> GetUserRolesAsync(Guid userId, CancellationToken ct);

    /// <summary>
    /// 為使用者新增一個角色
    /// </summary>
    Task AddUserRoleAsync(Guid userId, string roleId, DateTime? expireDate, CancellationToken ct);

    /// <summary>
    /// 移除使用者的特定角色
    /// </summary>
    Task RemoveUserRoleAsync(Guid userId, string roleId, CancellationToken ct);
}
