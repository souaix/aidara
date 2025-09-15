using Backend.Application.ViewModels.Services;

public interface IServiceStatService
{
    Task<List<ServiceStatVm>> GetServiceStatsAsync(Guid itemId, CancellationToken ct);
}
