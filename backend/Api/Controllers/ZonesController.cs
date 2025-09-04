using Microsoft.AspNetCore.Mvc;
using Dapper;
using Npgsql;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ZonesController : ControllerBase
{
    private readonly NpgsqlDataSource _ds;
    public ZonesController(NpgsqlDataSource ds) => _ds = ds;

    [HttpGet("with-count")]
    public async Task<ActionResult<IEnumerable<object>>> WithCount(
        double minLon, double minLat, double maxLon, double maxLat,
        CancellationToken ct)
    {
        var sql = """
        WITH b AS (SELECT ST_MakeEnvelope(@x1,@y1,@x2,@y2,4326) env)
        SELECT
            z.id,
            z.code,
            z.name,
            ST_X(ST_Centroid(z.geom)) AS lon,
            ST_Y(ST_Centroid(z.geom)) AS lat,
            COUNT(l.id) AS count
        FROM zones z, b
        LEFT JOIN listings l
          ON ST_Contains(z.geom, l.point)
        WHERE z.geom && b.env AND ST_Intersects(z.geom, b.env)
        GROUP BY z.id, z.code, z.name, z.geom
        ORDER BY z.id;
        """;

        await using var conn = await _ds.OpenConnectionAsync(ct);
        var rows = await conn.QueryAsync(sql, new
        {
            x1 = minLon,
            y1 = minLat,
            x2 = maxLon,
            y2 = maxLat
        });

        return Ok(rows);
    }
}
