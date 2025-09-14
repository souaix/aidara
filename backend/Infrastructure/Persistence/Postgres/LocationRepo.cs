using Backend.Application.Ports;
using Backend.Application.ViewModels.Services;
using Dapper;
using Npgsql;

public class LocationRepo : ILocationRepo
{
    private readonly NpgsqlDataSource _ds;

    public LocationRepo(NpgsqlDataSource ds)
    {
        _ds = ds;
    }

    public async Task<List<LocationCityVm>> GetCitiesAsync(CancellationToken ct)
    {
        const string sql = "SELECT city_id AS CityId, city_name AS CityName, code FROM location_city ORDER BY city_id;";
        using var conn = await _ds.OpenConnectionAsync(ct);
        var rows = await conn.QueryAsync<LocationCityVm>(sql);
        return rows.ToList();
    }

    public async Task<List<LocationDistrictVm>> GetDistrictsByCityAsync(int cityId, CancellationToken ct)
    {
        const string sql = @"SELECT district_id AS DistrictId, city_id AS CityId, district_name AS DistrictName, code
                             FROM location_district
                             WHERE city_id = @cityId
                             ORDER BY district_id;";
        using var conn = await _ds.OpenConnectionAsync(ct);
        var rows = await conn.QueryAsync<LocationDistrictVm>(sql, new { cityId });
        return rows.ToList();
    }
}
