namespace Frontend.Web.Models;

public enum AuthMode { Login, Register }

public record GoogleEnsurePayload(string Email, string? DisplayName, string? AvatarUrl, AuthMode Mode);

public record UserDto(Guid UserId, string Email, string? DisplayName, string? AvatarUrl);
