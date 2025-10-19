// Infrastructure/Persistence/Postgres/ServiceStatRepo.cs
using System.Data;
using Backend.Application.Ports;
using Backend.Application.ViewModels.Services;
using Dapper;

namespace Backend.Infrastructure.Persistence.Postgres;

public sealed class BossServiceStatRepo : IBossServiceStatRepo
{
    public async Task<List<ServiceStatVm>> GetServiceStatsAsync(IDbConnection conn, IDbTransaction? tx, Guid itemId, CancellationToken ct)
    {
        const string sql = """
            SELECT 
                COALESCE(area.city_id, addr.city_id) AS city_id,
                addr.district_id,
                COUNT(DISTINCT si.user_id) AS store_count
            FROM boss_service_item si
            LEFT JOIN boss_service_area area ON si.user_id = area.user_id
            LEFT JOIN boss_service_address addr ON si.user_id = addr.user_id
            WHERE si.item_id = @itemId
            GROUP BY COALESCE(area.city_id, addr.city_id), addr.district_id
            ORDER BY city_id, district_id;
        """;

        var rows = await conn.QueryAsync<ServiceStatVm>(
            new CommandDefinition(sql, new { itemId }, tx, cancellationToken: ct));

        return rows.ToList();
    }
}
