// Application/Services/Users/AuthService.cs
using System.Data;
using Backend.Application.Ports;
using Backend.Application.Shared;
using Backend.Contracts.User;
using Backend.Domain.Entities;

namespace Backend.Application.Services.Users;

public enum AuthMode { Login, Register }

public class AuthService
{
    private readonly IUnitOfWorkFactory _uowFactory;
    private readonly IUserRepo _userRepo;
    private readonly IUserRoleRepo _userRoleRepo;
    private readonly IUserActiveModeRepo _userActiveModeRepo;

    public AuthService(
        IUnitOfWorkFactory uowFactory,
        IUserRepo userRepo,
        IUserRoleRepo userRoleRepo,
        IUserActiveModeRepo userActiveModeRepo)
    {
        _uowFactory = uowFactory;
        _userRepo = userRepo;
        _userRoleRepo = userRoleRepo;
        _userActiveModeRepo = userActiveModeRepo;
    }

    public async Task<(UserDto User, bool IsNew)> EnsureUserForGoogleAutoAsync(
        string email,
        string? displayName,
        string? avatarUrl,
        CancellationToken ct)
    {
        await using var uow = await _uowFactory.BeginAsync(withTransaction: true, ct);

        try
        {
            // 1) 查詢既有使用者
            var existing = await _userRepo.GetByEmailAsync(uow.Connection, uow.Transaction, email, ct);
            if (existing is not null)
            {
                var roles = await _userRoleRepo.GetUserRolesAsync(uow.Connection, uow.Transaction, existing.UserId, ct);
                var activeMode = await _userActiveModeRepo.GetActiveModeAsync(uow.Connection, uow.Transaction, existing.UserId, ct)
                ?? "CUSTOMER";

                var dto = new UserDto
                {
                    UserId = existing.UserId,
                    Email = existing.Email,
                    DisplayName = existing.DisplayName,
                    AvatarUrl = existing.AvatarUrl,
                    Roles = roles.Select(r => r.RoleId).ToList(),
                    BossName = existing.BossName,
                    ActiveMode = activeMode
                };

                await _userRepo.TouchLastSeenAsync(uow.Connection, uow.Transaction, existing.UserId, ct);
                await uow.CommitAsync(ct);
                return (dto, false);
            }

            // 2) 新使用者建立
            var now = DateTime.UtcNow;
            var created = await _userRepo.InsertAsync(uow.Connection, uow.Transaction, new User
            {
                UserId = Guid.NewGuid(),
                Email = email,
                DisplayName = displayName ?? email,
                AvatarUrl = avatarUrl ?? "",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                BossName = displayName ?? email
            }, ct);

            // 3) 給預設角色
            await _userRoleRepo.AddUserRoleAsync(uow.Connection, uow.Transaction, created.UserId, "CUSTOMER", null, ct);

            // 4) 回傳 DTO
            var newDto = new UserDto
            {
                UserId = created.UserId,
                Email = created.Email,
                DisplayName = created.DisplayName,
                AvatarUrl = created.AvatarUrl,
                Roles = new List<string> { "CUSTOMER" },
                BossName = created.DisplayName,
                ActiveMode = "CUSTOMER"
            };

            await uow.CommitAsync(ct);
            return (newDto, true);
        }
        catch
        {
            await uow.RollbackAsync(ct);
            throw;
        }
    }
}
