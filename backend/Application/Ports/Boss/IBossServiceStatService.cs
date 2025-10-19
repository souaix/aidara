using Backend.Application.ViewModels.Services;

public interface IBossServiceStatService
{
    Task<List<ServiceStatVm>> GetServiceStatsAsync(Guid itemId, CancellationToken ct);
}
