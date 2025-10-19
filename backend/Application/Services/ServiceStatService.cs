// Application/Services/Services/ServiceStatService.cs
using Backend.Application.Ports;
using Backend.Application.Shared;
using Backend.Application.ViewModels.Services;

namespace Backend.Application.Services;

public sealed class ServiceStatService : IBossServiceStatService
{
    private readonly IUnitOfWorkFactory _uowFactory;
    private readonly IBossServiceStatRepo _repo;

    public ServiceStatService(IUnitOfWorkFactory uowFactory, IBossServiceStatRepo repo)
    {
        _uowFactory = uowFactory;
        _repo = repo;
    }

    public async Task<List<ServiceStatVm>> GetServiceStatsAsync(Guid itemId, CancellationToken ct)
    {
        await using var uow = await _uowFactory.BeginAsync(withTransaction: false, ct);
        var stats = await _repo.GetServiceStatsAsync(uow.Connection, uow.Transaction, itemId, ct);
        return stats;
    }
}
