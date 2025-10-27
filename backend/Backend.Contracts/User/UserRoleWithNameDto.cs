namespace Backend.Application.Contracts.User
{
    public class UserRoleWithNameDto
    {
        public Guid UserId { get; set; }
        public string RoleId { get; set; } = string.Empty;     // 給 switchMode 用
        public string RoleName { get; set; } = string.Empty;   // 顯示名稱
    }
}
