// Application/Services/Boss/BossInfoQuestionnaireService.cs
using Backend.Application.Ports;
using Backend.Application.Shared;
using Backend.Application.ViewModels.Services;

namespace Backend.Application.Services.Boss;

public class BossInfoQuestionnaireService
{
    private readonly IUnitOfWorkFactory _uowFactory;
    private readonly IBossInfoRepo _bossInfoRepo;
    private readonly IUserRoleRepo _userRoleRepo;

    public BossInfoQuestionnaireService(
        IUnitOfWorkFactory uowFactory,
        IBossInfoRepo bossInfoRepo,
        IUserRoleRepo userRoleRepo)
    {
        _uowFactory = uowFactory;
        _bossInfoRepo = bossInfoRepo;
        _userRoleRepo = userRoleRepo;
    }

    public async Task SubmitAsync(BossInfoQuestionnaireDto dto, CancellationToken ct)
    {
        await using var uow = await _uowFactory.BeginAsync(withTransaction: true, ct);

        try
        {
            // 1️⃣ 更新小老闆問卷資訊（完整覆蓋）
            await _bossInfoRepo.ReplaceItemsAsync(uow.Connection, uow.Transaction, dto.UserId, dto.ItemPriceRanges, ct);
            await _bossInfoRepo.ReplaceMethodsAsync(uow.Connection, uow.Transaction, dto.UserId, dto.ServiceMethods, ct);
            await _bossInfoRepo.ReplaceAreasAsync(uow.Connection, uow.Transaction, dto.UserId, dto.ServiceAreas, ct);
            await _bossInfoRepo.ReplaceAddressesAsync(uow.Connection, uow.Transaction, dto.UserId, dto.ServiceAddresses, ct);

            // 2️⃣ 更新使用者角色為 GOLDENBOSS
            await _userRoleRepo.AddUserRoleAsync(uow.Connection, uow.Transaction, dto.UserId, "GOLDENBOSS", null, ct);

            // ✅ 提交整個 transaction
            await uow.CommitAsync(ct);
        }
        catch (Exception ex)
        {
            await uow.RollbackAsync(ct);
            Console.WriteLine($"❗BossInfoQuestionnaireService Submit Failed: {ex.Message}");
            throw;
        }
    }
}
