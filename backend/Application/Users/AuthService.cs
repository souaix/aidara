using Backend.Application.Ports;
using Backend.Contracts.Users;
using Backend.Domain.Entities;

// 👇 新增
using Backend.Contracts.Wallet;  // CreateLedgerRequest
								 // 若你把 IWalletRepo 放在 Ports：using Backend.Application.Ports;

namespace Backend.Application.Users;

public enum AuthMode { Login, Register }

public class AuthService
{
	private readonly IUnitOfWork _uow;
	public AuthService(IUnitOfWork uow) => _uow = uow;

	// 簽名不變
	public async Task<UserDto?> EnsureUserForGoogleAsync(
		string email, string? displayName, string? avatarUrl,
		AuthMode mode, Func<IUserRepo> repoFactory, CancellationToken ct)
	{
		await _uow.BeginAsync(ct);
		try
		{
			var users = repoFactory();

			// 1) 已存在 → 僅更新最後登入
			var existing = await users.GetByEmailAsync(email, ct);
			if (existing is not null)
			{
				await users.TouchLastSeenAsync(existing.UserId, ct);
				await _uow.CommitAsync(ct);
				return ToDto(existing);
			}

			// 2) 不存在且是 Login 模式 → 不建立
			if (mode == AuthMode.Login)
			{
				await _uow.RollbackAsync();
				return null;
			}

			// 3) Register 模式 → 建新帳號
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

			// 4) 🎁 新帳號贈點：Silver 99,999（在同一個交易裡）
			var wallet = _uow.CreateWalletRepo();
			await wallet.InsertAsync(new CreateLedgerRequest(
				UserId: created.UserId,
				Currency: "Silver",
				Amount: 99999,              // 單位＝點數/分（你現在用 int）
				TxType: "DEPOSIT",          // 我們已全面用 string，不用 enum
				OrderId: null,
				Meta: "{\"reason\":\"signup_bonus\"}",
				RejectNegative: false        // 純加值，不會變負
			), ct);

			// 5) 交易完成
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
