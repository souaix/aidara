using Backend.Application.Ports;
using Backend.Application.ViewModels.Services;

public class ServiceStatService : IServiceStatService
{
    private readonly IServiceStatRepo _repo;

    public ServiceStatService(IServiceStatRepo repo)
    {
        _repo = repo;
    }

    public Task<List<ServiceStatVm>> GetServiceStatsAsync(Guid itemId, CancellationToken ct)
    {
        return _repo.GetServiceStatsAsync(itemId, ct);
    }
}
