// Infrastructure/Persistence/Postgres/BossInfoRepo.cs
using System.Data;
using Backend.Application.Ports;
using Backend.Application.ViewModels.Services;
using Dapper;

namespace Backend.Infrastructure.Persistence.Postgres;

public sealed class BossInfoRepo : IBossInfoRepo
{
    public async Task ReplaceItemsAsync(IDbConnection conn, IDbTransaction? tx, Guid userId, List<ItemPriceRangeDto> items, CancellationToken ct)
    {
        await conn.ExecuteAsync(
            new CommandDefinition("DELETE FROM boss_service_item WHERE user_id = @userId", new { userId }, tx, cancellationToken: ct));

        foreach (var item in items)
        {
            const string sql = """
                INSERT INTO boss_service_item (id, user_id, item_id, min_price, max_price, created_at)
                VALUES (gen_random_uuid(), @userId, @ItemId, @MinPrice, @MaxPrice, now());
            """;

            await conn.ExecuteAsync(
                new CommandDefinition(sql, new { userId, item.ItemId, item.MinPrice, item.MaxPrice }, tx, cancellationToken: ct));
        }
    }

    public async Task ReplaceMethodsAsync(IDbConnection conn, IDbTransaction? tx, Guid userId, List<string> methods, CancellationToken ct)
    {
        await conn.ExecuteAsync(
            new CommandDefinition("DELETE FROM boss_service_method WHERE user_id = @userId", new { userId }, tx, cancellationToken: ct));

        foreach (var method in methods)
        {
            const string sql = """
                INSERT INTO boss_service_method (id, user_id, method, updated_at)
                VALUES (gen_random_uuid(), @userId, @method, now());
            """;

            await conn.ExecuteAsync(
                new CommandDefinition(sql, new { userId, method }, tx, cancellationToken: ct));
        }
    }

    public async Task ReplaceAreasAsync(
        IDbConnection conn,
        IDbTransaction? tx,
        Guid userId,
        List<ServiceAreaDto> areas,
        CancellationToken ct)
    {
        // 先刪除舊資料
        await conn.ExecuteAsync(
            new CommandDefinition("DELETE FROM boss_service_area WHERE user_id = @userId",
            new { userId }, tx, cancellationToken: ct));

        if (areas is null || areas.Count == 0)
            return;

        // 先查詢該使用者有哪些服務項目 (因為要展開)
        const string itemSql = "SELECT item_id FROM boss_service_item WHERE user_id = @userId;";
        var itemIds = (await conn.QueryAsync<string>(
            new CommandDefinition(itemSql, new { userId }, tx, cancellationToken: ct))).ToList();

        if (itemIds.Count == 0)
            return; // 沒有服務項目就不展開

        // 寫入多筆
        const string insertSql = """
        INSERT INTO boss_service_area
            (id, user_id, item_id, city_id, district_id, postal_id, created_at)
        VALUES
            (gen_random_uuid(), @UserId, @ItemId, @CityId, @DistrictId, @PostalId, now())
        ON CONFLICT (user_id, item_id, postal_id) DO NOTHING;
    """;

        // 組合展開資料
        var expanded = new List<object>();
        foreach (var itemId in itemIds)
        {
            foreach (var area in areas)
            {
                expanded.Add(new
                {
                    UserId = userId,
                    ItemId = itemId, // string
                    CityId = area.CityId,
                    DistrictId = area.DistrictId,
                    PostalId = area.PostalId
                });
            }
        }

        await conn.ExecuteAsync(
            new CommandDefinition(insertSql, expanded, tx, cancellationToken: ct));
    }

    public async Task ReplaceAddressesAsync(IDbConnection conn, IDbTransaction? tx, Guid userId, List<ServiceAddressDto> addresses, CancellationToken ct)
    {
        await conn.ExecuteAsync(
            new CommandDefinition("DELETE FROM boss_service_address WHERE user_id = @userId", new { userId }, tx, cancellationToken: ct));

        foreach (var addr in addresses)
        {
            const string sql = """
                INSERT INTO boss_service_address 
                    (id, user_id, city_id, district_id, street, address_no, created_at, updated_at)
                VALUES 
                    (gen_random_uuid(), @userId, @CityId, @DistrictId, @Street, @AddressNo, now(), now());
            """;

            await conn.ExecuteAsync(
                new CommandDefinition(sql, new { userId, addr.CityId, addr.DistrictId, addr.Street, addr.AddressNo }, tx, cancellationToken: ct));
        }
    }
}
