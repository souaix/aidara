using Backend.Application.ViewModels.Services;

public interface ILocationRepo
{
    Task<List<LocationCityVm>> GetCitiesAsync(CancellationToken ct);
    Task<List<LocationDistrictVm>> GetDistrictsByCityAsync(int cityId, CancellationToken ct);
}
