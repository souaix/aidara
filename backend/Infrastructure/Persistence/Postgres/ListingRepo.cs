using Backend.Application.Ports;
using Backend.Domain.Geo;
using Backend.Domain.Listings;
using Dapper;
using Npgsql;

namespace Backend.Infrastructure.Persistence.Postgres;

public sealed class ListingRepo : IListingRepo
{
    private readonly NpgsqlDataSource _ds;
    public ListingRepo(NpgsqlDataSource ds) => _ds = ds;

    public async Task<(IReadOnlyList<Listing> Items, int Total)> SearchAsync(
        BBox bbox,
        decimal? minUnit,
        decimal? maxUnit,
        int? minRooms,
        int? maxRooms,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        await using var conn = await _ds.OpenConnectionAsync(ct);

        var args = new
        {
            x1 = bbox.MinLon,
            y1 = bbox.MinLat,
            x2 = bbox.MaxLon,
            y2 = bbox.MaxLat,
            minU = minUnit,
            maxU = maxUnit,
            minR = minRooms,
            maxR = maxRooms,
            offset = (page - 1) * pageSize,
            limit = pageSize
        };

        var total = await conn.ExecuteScalarAsync<int>(
            """
            WITH b AS (SELECT ST_MakeEnvelope(@x1,@y1,@x2,@y2,4326) env)
            SELECT COUNT(*) FROM listings, b
            WHERE point && env
              AND ST_Intersects(point, env)
              AND (@minU IS NULL OR price_unit >= @minU)
              AND (@maxU IS NULL OR price_unit <= @maxU)
              AND (@minR IS NULL OR rooms >= @minR)
              AND (@maxR IS NULL OR rooms <= @maxR);
            """, args);

        var items = (await conn.QueryAsync<Listing>(
            """
            WITH b AS (SELECT ST_MakeEnvelope(@x1,@y1,@x2,@y2,4326) env)
            SELECT id, title, price_unit AS PriceUnit, rooms, lon, lat
            FROM listings, b
            WHERE point && env
              AND ST_Intersects(point, env)
              AND (@minU IS NULL OR price_unit >= @minU)
              AND (@maxU IS NULL OR price_unit <= @maxU)
              AND (@minR IS NULL OR rooms >= @minR)
              AND (@maxR IS NULL OR rooms <= @maxR)
            ORDER BY id
            OFFSET @offset LIMIT @limit;
            """, args)).AsList();

        return (items, total);
    }
}
