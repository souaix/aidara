// Infrastructure/Persistence/Postgres/LocationRepo.cs
using System.Data;
using Backend.Application.Ports;
using Backend.Application.ViewModels.Services;
using Dapper;

namespace Backend.Infrastructure.Persistence.Postgres;

public sealed class LocationRepo : ILocationRepo
{
    public async Task<List<LocationCityVm>> GetCitiesAsync(IDbConnection conn, IDbTransaction? tx, CancellationToken ct)
    {
        const string sql = """
            SELECT city_id AS CityId, city_name AS CityName, code
            FROM location_city
            ORDER BY city_id;
        """;

        var rows = await conn.QueryAsync<LocationCityVm>(
            new CommandDefinition(sql, transaction: tx, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task<List<LocationDistrictVm>> GetDistrictsByCityAsync(IDbConnection conn, IDbTransaction? tx, int cityId, CancellationToken ct)
    {
        const string sql = """
            SELECT district_id AS DistrictId, city_id AS CityId, district_name AS DistrictName, code
            FROM location_district
            WHERE city_id = @cityId
            ORDER BY district_id;
        """;

        var rows = await conn.QueryAsync<LocationDistrictVm>(
            new CommandDefinition(sql, new { cityId }, tx, cancellationToken: ct));
        return rows.ToList();
    }
}
