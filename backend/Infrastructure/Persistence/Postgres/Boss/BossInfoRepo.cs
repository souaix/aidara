// Infrastructure/Persistence/Postgres/BossInfoRepo.cs
using Backend.Application.Ports;
using Backend.Application.ViewModels.Boss;
using Backend.Application.ViewModels.Services;
using Dapper;
using System.Data;
using System.Numerics;
using System.Text.Json;

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

    public async Task ReplaceAreasAsync(IDbConnection conn, IDbTransaction? tx,Guid userId,List<ServiceAreaDto> areas,CancellationToken ct)
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
            (id, user_id, item_id, city_id, district_id, postal_id, note, created_at)
        VALUES
            (gen_random_uuid(), @UserId, @ItemId, @CityId, @DistrictId, @PostalId, @Note, now())
        ON CONFLICT (user_id, item_id, city_id, district_id, postal_id) DO NOTHING;
    """; // user_id, item_id, postal_id 改 user_id, item_id, city_id, district_id, postal_id

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
                    PostalId = area.PostalId == 0 ? null : area.PostalId,
                    Note = area.Note
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
                    (id, user_id, city_id, district_id, street, address_no, postal_id, phone, contact_name, created_at, updated_at)
                VALUES 
                    (gen_random_uuid(), @userId, @CityId, @DistrictId, @Street, @AddressNo, @PostalId, @Phone, @ContactName, now(), now());
            """;

            var postalId = addr.PostalId == 0 ? null : addr.PostalId;

            await conn.ExecuteAsync(
                new CommandDefinition(sql, new { userId, addr.CityId, addr.DistrictId, addr.Street, addr.AddressNo, postalId, addr.Phone, addr.ContactName }, tx, cancellationToken: ct));
        }
    }
   
    public async Task UpsertItemAsync(
        IDbConnection conn, IDbTransaction? tx,
        Guid userId, ItemPriceRangeDto item, CancellationToken ct)
    {
        const string sql = """
        INSERT INTO boss_service_item (id, user_id, item_id, min_price, max_price, created_at, is_active)
        VALUES (gen_random_uuid(), @UserId, @ItemId, @MinPrice, @MaxPrice, now(), @IsActive)
        ON CONFLICT (user_id, item_id)
        DO UPDATE SET 
            min_price = EXCLUDED.min_price,
            max_price = EXCLUDED.max_price,
            is_active = EXCLUDED.is_active;
    """;

        await conn.ExecuteAsync(new CommandDefinition(
            sql,
            new { UserId = userId, item.ItemId, item.MinPrice, item.MaxPrice, item.IsActive },
            tx,
            cancellationToken: ct));
    }

    public async Task DeleteItemAsync(
    IDbConnection conn, IDbTransaction? tx,
    Guid userId, string itemId, CancellationToken ct)
    {
        await conn.ExecuteAsync(
            new CommandDefinition(
                "DELETE FROM boss_service_item WHERE user_id = @userId AND item_id = @itemId", 
                new { userId, itemId }, 
                tx, 
                cancellationToken: ct));
    }

    public async Task ReplaceAreasForItemAsync(
        IDbConnection conn, IDbTransaction? tx,
        Guid userId, string itemId, List<ServiceAreaDto> areas, CancellationToken ct)
    {
        const string deleteSql = """
        DELETE FROM boss_service_area
        WHERE user_id = @UserId AND item_id = @ItemId;
    """;

        await conn.ExecuteAsync(new CommandDefinition(deleteSql, new { UserId = userId, ItemId = itemId }, tx, cancellationToken: ct));

        if (areas == null || areas.Count == 0)
            return;

        const string insertSql = """
        INSERT INTO boss_service_area
            (id, user_id, item_id, city_id, district_id, postal_id, created_at)
        VALUES
            (gen_random_uuid(), @UserId, @ItemId, @CityId, @DistrictId, @PostalId, now());
    """;

        foreach (var a in areas)
        {
            await conn.ExecuteAsync(new CommandDefinition(
                insertSql,
                new
                {
                    UserId = userId,
                    ItemId = itemId,
                    a.CityId,
                    a.DistrictId,
                    a.PostalId
                },
                tx,
                cancellationToken: ct));
        }
    }

    public async Task UpsertBossServiceAsync(
        IDbConnection conn, IDbTransaction? tx,
        BossServiceUpsertDto dto, CancellationToken ct)
    {
        // Step 1: upsert 單筆 item
        var item = new ItemPriceRangeDto
        {
            ItemId = dto.ItemId,
            MinPrice = dto.MinPrice,
            MaxPrice = dto.MaxPrice
        };
        await UpsertItemAsync(conn, tx, dto.UserId, item, ct);

        // Step 2: 更新該 item 的範圍
        await ReplaceAreasForItemAsync(conn, tx, dto.UserId, dto.ItemId, dto.ServiceAreas, ct);

        // Step 3: 全域方法/地址不動
    }

}
