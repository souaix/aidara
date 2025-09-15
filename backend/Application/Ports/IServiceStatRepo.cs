using Backend.Application.ViewModels.Services;

public interface IServiceStatRepo
{
    Task<List<ServiceStatVm>> GetServiceStatsAsync(Guid itemId, CancellationToken ct);
}
