// Application/Ports/ILocationRepo.cs
using System.Data;
using Backend.Application.ViewModels.Services;

namespace Backend.Application.Ports;

public interface ILocationRepo
{
    Task<List<LocationCityVm>> GetCitiesAsync(IDbConnection conn, IDbTransaction? tx, CancellationToken ct);
    Task<List<LocationDistrictVm>> GetDistrictsByCityAsync(IDbConnection conn, IDbTransaction? tx, int cityId, CancellationToken ct);
}
