// Application/Ports/ILocationRepo.cs
using Backend.Application.ViewModels;
using System.Data;
using System.Threading.Tasks;

namespace Backend.Application.Ports;

public interface ILocationRepo
{
    Task<List<LocationCityVm>> GetCitiesAsync(IDbConnection conn, IDbTransaction? tx, CancellationToken ct);
    Task<List<LocationDistrictVm>> GetDistrictsByCityAsync(IDbConnection conn, IDbTransaction? tx, int cityId, CancellationToken ct);
    Task<List<LocationPostalVm>> GetPostalByDistrictAsync(IDbConnection conn, IDbTransaction? tx, int districtId, CancellationToken ct);

    Task<List<LocationPostalVm>> GetPostalsByDistrictsAsync(IDbConnection conn, IDbTransaction? tx, IEnumerable<int> districtIds, CancellationToken ct);
}
