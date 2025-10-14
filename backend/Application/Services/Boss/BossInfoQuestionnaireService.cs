using Backend.Application.Ports;
using Backend.Application.ViewModels.Services;

namespace Backend.Application.Services.Boss;

public class BossInfoQuestionnaireService
{
	private readonly Func<IUnitOfWork> _uowFactory;

	public BossInfoQuestionnaireService(Func<IUnitOfWork> uowFactory)
	{
		_uowFactory = uowFactory;
	}

	public async Task SubmitAsync(BossInfoQuestionnaireDto dto, CancellationToken ct)
	{
		await using var uow = _uowFactory();
		await uow.BeginAsync(ct);

		try
		{
			// 1️⃣ 更新小老闆問卷資訊
			var repo = uow.CreateBossInfoRepo();
			await repo.ReplaceItemsAsync(dto.UserId, dto.ItemPriceRanges, ct);
			await repo.ReplaceMethodsAsync(dto.UserId, dto.ServiceMethods, ct);
			await repo.ReplaceAreasAsync(dto.UserId, dto.ServiceAreas, ct);
			await repo.ReplaceAddressesAsync(dto.UserId, dto.ServiceAddresses, ct);

			// 2️⃣ 更新使用者角色為 GOLDENBOSS
			var roleRepo = uow.CreateUserRepo();
			await roleRepo.AddUserRoleAsync(dto.UserId, "GOLDENBOSS", null, ct);

			// ✅ 提交整個 transaction
			await uow.CommitAsync(ct);
		}
		catch (Exception ex)
		{
			await uow.RollbackAsync();
			Console.WriteLine($"❗BossInfoQuestionnaireService Submit Failed: {ex.Message}");
			throw;
		}
	}
}
