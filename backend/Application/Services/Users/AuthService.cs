using Backend.Application.Ports;
using Backend.Contracts.Users;
using Backend.Contracts.Wallet;
using Backend.Domain.Entities;

namespace Backend.Application.Services.Users;

public enum AuthMode { Login, Register }

public class AuthService
{
	private readonly IUnitOfWork _uow;
	public AuthService(IUnitOfWork uow) => _uow = uow;

	public async Task<(UserDto User, bool IsNew)> EnsureUserForGoogleAutoAsync(
		string email, string? displayName, string? avatarUrl,
		Func<IUserRoleRepo> repoFactory, CancellationToken ct)
	{
		await _uow.BeginAsync(ct);
		try
		{
			var users = repoFactory();

			// 1) 既有使用者：回傳含角色的 UserDto
			var existing = await users.GetByEmailAsync(email, ct);

			if (existing is not null)
			{
				var roleRepo = _uow.CreateUserRepo();
				var roles = await roleRepo.GetUserRolesAsync(existing.UserId, ct);
				var dto = new UserDto
				{
					UserId = existing.UserId,
					Email = existing.Email,
					DisplayName = existing.DisplayName,
					AvatarUrl = existing.AvatarUrl,
					Roles = roles.Select(r => r.RoleId).ToList()
				};

				await users.TouchLastSeenAsync(existing.UserId, ct);
				await _uow.CommitAsync(ct);

				return (dto, false);
			}

			// 2) 新使用者：建立帳號
			var now = DateTime.UtcNow;
			var created = await users.InsertAsync(new User
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

			// 3) 發錢包（依你原本邏輯）
			var wallet = _uow.CreateWalletRepo();
			await wallet.InsertAsync(new CreateLedgerRequest(
				UserId: created.UserId,
				Currency: "Silver",
				Amount: 99999,
				TxType: "DEPOSIT",
				OrderId: null,
				Meta: "{\"reason\":\"signup_bonus\"}",
				RejectNegative: false
			), ct);

			// 4) 給預設角色（可依需求改：CUSTOMER / UNVERIFYBOSS 等）
			var userRoleRepo = _uow.CreateUserRepo();
			await userRoleRepo.AddUserRoleAsync(created.UserId, "CUSTOMER", null, ct);

			// 5) 組含角色的 UserDto
			var newUserRoles = new List<string> { "CUSTOMER" };
			var newDto = new UserDto
			{
				UserId = created.UserId,
				Email = created.Email,
				DisplayName = created.DisplayName,
				AvatarUrl = created.AvatarUrl,
				Roles = newUserRoles,
				BossName = created.DisplayName
			};

			await _uow.CommitAsync(ct);
			return (newDto, true);
		}
		catch
		{
			await _uow.RollbackAsync();
			throw;
		}
	}
}
