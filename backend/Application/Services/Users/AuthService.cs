using Backend.Application.Ports;
using Backend.Contracts.Users;

using Backend.Domain.Entities;

namespace Backend.Application.Services.Users;

public enum AuthMode { Login, Register }

public class AuthService
{
    private readonly IUserRepo _userRepo;
    private readonly IUserRoleRepo _userRoleRepo;


    public AuthService(
        IUserRepo userRepo,
        IUserRoleRepo userRoleRepo)
    {
        _userRepo = userRepo;
        _userRoleRepo = userRoleRepo;
      
    }

    public async Task<(UserDto User, bool IsNew)> EnsureUserForGoogleAutoAsync(
        IUowContext uow,
        string email,
        string? displayName,
        string? avatarUrl,
        CancellationToken ct)
    {
        // 1) 既有使用者
        var existing = await _userRepo.GetByEmailAsync(uow, email, ct);
        if (existing is not null)
        {
            var roles = await _userRoleRepo.GetUserRolesAsync(uow, existing.UserId, ct);

            var dto = new UserDto
            {
                UserId = existing.UserId,
                Email = existing.Email,
                DisplayName = existing.DisplayName,
                AvatarUrl = existing.AvatarUrl,
                Roles = roles.Select(r => r.RoleId).ToList(),
                BossName = existing.BossName
            };

            await _userRepo.TouchLastSeenAsync(uow, existing.UserId, ct);
            return (dto, false);
        }

        // 2) 新使用者
        var now = DateTime.UtcNow;
        var created = await _userRepo.InsertAsync(uow, new User
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

        // 3) 建立錢包
        //await _walletRepo.InsertAsync(uow, new CreateLedgerRequest(
        //    UserId: created.UserId,
        //    Currency: "Silver",
        //    Amount: 99999,
        //    TxType: "DEPOSIT",
        //    OrderId: null,
        //    Meta: "{\"reason\":\"signup_bonus\"}",
        //    RejectNegative: false
        //), ct);

        // 4) 給預設角色
        await _userRoleRepo.AddUserRoleAsync(uow, created.UserId, "CUSTOMER", null, ct);

        // 5) 組含角色的 UserDto
        var newDto = new UserDto
        {
            UserId = created.UserId,
            Email = created.Email,
            DisplayName = created.DisplayName,
            AvatarUrl = created.AvatarUrl,
            Roles = new List<string> { "CUSTOMER" },
            BossName = created.DisplayName
        };

        return (newDto, true);
    }
}
