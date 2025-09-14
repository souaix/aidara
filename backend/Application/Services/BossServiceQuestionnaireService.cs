using Backend.Application.Ports;
using Backend.Application.ViewModels.Services;
namespace Backend.Application.Services;
public class BossServiceQuestionnaireService
{
    private readonly Func<IUnitOfWork> _uowFactory;

    public BossServiceQuestionnaireService(Func<IUnitOfWork> uowFactory)
    {
        _uowFactory = uowFactory;
    }

    public async Task SubmitAsync(BossServiceQuestionnaireDto dto, CancellationToken ct)
    {
        await using var uow = _uowFactory();
        await uow.BeginAsync(ct);

        try
        {
            var repo = uow.CreateBossServiceRepo();

            await repo.ReplaceItemsAsync(dto.UserId, dto.ServiceItemIds, ct);
            await repo.ReplaceMethodsAsync(dto.UserId, dto.ServiceMethods, ct);
            await repo.ReplaceAreasAsync(dto.UserId, dto.ServiceAreas, ct);
            await repo.ReplaceAddressesAsync(dto.UserId, dto.ServiceAddresses, ct);

            await uow.CommitAsync(ct);
        }
        catch
        {
            await uow.RollbackAsync();
            throw;
        }
    }
}
