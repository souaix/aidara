using Backend.Application.Ports;
using Backend.Application.ViewModels.Services;
namespace Backend.Application.Services;
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
            var repo = uow.CreateBossInfoRepo();
            await repo.ReplaceItemsAsync(dto.UserId, dto.ItemPriceRanges, ct);            
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
