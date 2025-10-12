namespace Backend.Contracts.Users;

public class UserDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;

    // ✅ 新增
    public List<string> Roles { get; set; } = new();
}

