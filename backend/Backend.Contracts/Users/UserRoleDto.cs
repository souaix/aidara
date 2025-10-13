namespace Backend.Application.Contracts.Users;

/// <summary>
/// 使用者角色資料傳輸物件
/// </summary>
public class UserRoleDto
{
    /// <summary>使用者 ID (UUID)</summary>
    public Guid UserId { get; set; }

    /// <summary>角色代碼（例如 ADMIN、BOSS、CUSTOMER）</summary>
    public string RoleId { get; set; } = string.Empty;

    public string RoleName { get; set; } = "";

    /// <summary>建立時間</summary>
    public DateTime CreateDate { get; set; }

    /// <summary>更新時間</summary>
    public DateTime UpdateDate { get; set; }

    /// <summary>角色到期時間（可為 null 表示永久有效）</summary>
    public DateTime? ExpireDate { get; set; }
}

/// <summary>
/// 角色主檔資料傳輸物件
/// </summary>
public class RoleBasisDto
{
    /// <summary>角色代碼</summary>
    public string RoleId { get; set; } = string.Empty;

    /// <summary>角色名稱</summary>
    public string RoleName { get; set; } = string.Empty;

    /// <summary>角色說明</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>是否啟用</summary>
    public bool IsActive { get; set; }

    /// <summary>建立時間</summary>
    public DateTime CreateDate { get; set; }

    /// <summary>更新時間</summary>
    public DateTime UpdateDate { get; set; }
}
