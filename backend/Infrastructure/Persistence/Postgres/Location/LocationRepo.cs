// Infrastructure/Persistence/Postgres/LocationRepo.cs
using System.Data;
using Backend.Application.Ports;
using Backend.Application.ViewModels;
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

    public async Task<List<LocationDistrictVm>> GetDistrictsByCitiesAsync(IDbConnection conn, IDbTransaction? tx,IEnumerable<int> cityIds, CancellationToken ct)
    {
        const string sql = """
        SELECT district_id AS DistrictId,
               city_id AS CityId,
               district_name AS DistrictName,
               code
        FROM location_district
        WHERE city_id = ANY(@cityIds)
        ORDER BY city_id, district_id;
    """;

        var rows = await conn.QueryAsync<LocationDistrictVm>(
            new CommandDefinition(sql, new { cityIds }, tx, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task<List<LocationPostalVm>> GetPostalByDistrictAsync(IDbConnection conn, IDbTransaction? tx, int districtId, CancellationToken ct)
    {
        const string sql = """
            SELECT postal_id AS PostalId,
                   district_id AS DistrictId,
                   postal_code AS PostalCode,
                   locality AS Locality
            FROM location_postal
            WHERE district_id = @districtId
            ORDER BY postal_id;
        """;

        var rows = await conn.QueryAsync<LocationPostalVm>(
            new CommandDefinition(sql, new { districtId }, tx, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task<List<LocationPostalVm>> GetPostalsByDistrictsAsync(IDbConnection conn, IDbTransaction? tx, IEnumerable<int> districtIds, CancellationToken ct)
    {
        const string sql = """
        SELECT postal_id   AS PostalId,
               district_id AS DistrictId,
               postal_code AS PostalCode,
               locality    AS Locality
        FROM location_postal
        WHERE district_id = ANY(@districtIds)
        ORDER BY district_id, postal_id;
    """;

        var rows = await conn.QueryAsync<LocationPostalVm>(
            new CommandDefinition(sql, new { districtIds }, tx, cancellationToken: ct));

        return rows.ToList();
    }
}
