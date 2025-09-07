namespace Backend.Contracts.Users;

public record UserDto(
    Guid UserId,
    string Email,
    string? DisplayName,
    string? AvatarUrl
);
