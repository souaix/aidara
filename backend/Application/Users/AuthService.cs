using Backend.Application.Ports;
using Backend.Contracts.Users;
using Backend.Domain.Entities;

namespace Backend.Application.Users;

public enum AuthMode { Login, Register }

public class AuthService
{
    private readonly IUnitOfWork _uow;
    public AuthService(IUnitOfWork uow) => _uow = uow;

    // 簽名：6 參數（email, displayName, avatarUrl, mode, repoFactory, ct）
    public async Task<UserDto?> EnsureUserForGoogleAsync(
        string email, string? displayName, string? avatarUrl,
        AuthMode mode, Func<IUserRepo> repoFactory, CancellationToken ct)
    {
        await _uow.BeginAsync(ct);
        try
        {
            var users = repoFactory();

            var existing = await users.GetByEmailAsync(email, ct);
            if (existing is not null)
            {
                await users.TouchLastSeenAsync(existing.UserId, ct);
                await _uow.CommitAsync(ct);
                return ToDto(existing);
            }

            if (mode == AuthMode.Login)
            {
                await _uow.RollbackAsync();
                return null;
            }

            var now = DateTime.UtcNow;
            var created = await users.InsertAsync(new User
            {
                UserId = Guid.NewGuid(),
                Email = email,
                DisplayName = displayName,
                AvatarUrl = avatarUrl,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            }, ct);

            await _uow.CommitAsync(ct);
            return ToDto(created);
        }
        catch
        {
            await _uow.RollbackAsync();
            throw;
        }
    }

    private static UserDto ToDto(User u) =>
        new(u.UserId, u.Email, u.DisplayName, u.AvatarUrl);
}
