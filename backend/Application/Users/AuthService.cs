using Backend.Application.Ports;
using Backend.Contracts.Users;
// 👇 新增
using Backend.Contracts.Wallet;  // CreateLedgerRequest
using Backend.Domain.Entities;
								 // 若你把 IWalletRepo 放在 Ports：using Backend.Application.Ports;

namespace Backend.Application.Users;

public enum AuthMode { Login, Register }

public class AuthService
{
	private readonly IUnitOfWork _uow;
	public AuthService(IUnitOfWork uow) => _uow = uow;

    // 簽名不變
    public async Task<(UserDto User, bool IsNew)> EnsureUserForGoogleAutoAsync(
        string email, string? displayName, string? avatarUrl,
        Func<IUserRepo> repoFactory, CancellationToken ct)
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
                return (ToDto(existing), false);
            }

			// ✅ 若不存在就直接建帳號
			var now = DateTime.UtcNow;
			var created = await users.InsertAsync(new User
			{
				UserId = Guid.NewGuid(),
				Email = email,
				DisplayName = displayName ?? email,
				AvatarUrl = avatarUrl ?? "",
				IsActive = true,
				CreatedAt = now,
				UpdatedAt = now
			}, ct);

			// 🎁 送錢包（可選）
			try
			{
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
			}
			catch (Exception ex)
			{
				Console.WriteLine($"❗Wallet insert failed: {ex.Message}");
				throw;  // 還是要 rollback
			}



			await _uow.CommitAsync(ct);
            return (ToDto(created), true);
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
