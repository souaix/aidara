using Backend.Application.Contracts.Users;
using Backend.Domain.Entities;

namespace Backend.Application.Ports;

public interface IUserRepo
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct);
    Task<User> InsertAsync(User user, CancellationToken ct);
    Task TouchLastSeenAsync(Guid userId, CancellationToken ct);

    // 使用者完整資料（含 boss_name）
    Task<User?> GetUserProfileAsync(Guid userId, CancellationToken ct);

    // ✅ 回傳角色的 id + name（供頁面顯示/判斷）
    Task<UserRoleDto?> GetUserRoleAsync(Guid userId, CancellationToken ct);

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
