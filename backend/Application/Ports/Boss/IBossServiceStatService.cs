using Backend.Application.ViewModels.Services;

namespace Backend.Application.Ports; // 新增

public interface IBossServiceStatService
{
    Task<List<ServiceStatVm>> GetServiceStatsAsync(Guid itemId, CancellationToken ct);
}
