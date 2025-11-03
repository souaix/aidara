namespace Frontend.Web.Models;

public enum AuthMode { Login, Register }

public record GoogleEnsurePayload(string Email, string? DisplayName, string? AvatarUrl, AuthMode Mode);

public class UserDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string ActiveMode { get; init; } = "CUSTOMER";

    // ✅ 新增
    public List<string> Roles { get; set; } = new();
}

public class EnsureUserResponse
{
    public UserDto User { get; set; } = default!;
    public bool IsNew { get; set; }
}
