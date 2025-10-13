using Backend.Application.Ports;
using Backend.Application.ViewModels.Services;
using Dapper;
using Npgsql;

public class BossInfoRepo : IBossServiceRepo
{
    private readonly NpgsqlConnection _conn;
    private readonly NpgsqlTransaction _tx;

    public BossInfoRepo(NpgsqlConnection conn, NpgsqlTransaction tx)
    {
        _conn = conn;
        _tx = tx;
    }

    //問券結果取代資Boss資料
    public async Task ReplaceItemsAsync(Guid userId, List<ItemPriceRangeDto> items, CancellationToken ct)
    {
        await _conn.ExecuteAsync("DELETE FROM boss_service_item WHERE user_id = @userId", new { userId }, _tx);

        foreach (var item in items)
        {
            await _conn.ExecuteAsync(@"
            INSERT INTO boss_service_item (id, user_id, item_id, min_price, max_price, created_at)
            VALUES (gen_random_uuid(), @userId, @ItemId, @MinPrice, @MaxPrice, now())",
                new
                {
                    userId,
                    item.ItemId,
                    item.MinPrice,
                    item.MaxPrice
                },
                _tx);
        }
    }


    public async Task ReplaceMethodsAsync(Guid userId, List<string> methods, CancellationToken ct)
    {
        await _conn.ExecuteAsync("DELETE FROM boss_service_method WHERE user_id=@userId", new { userId }, _tx);

        foreach (var method in methods)
        {
            await _conn.ExecuteAsync(@"
                INSERT INTO boss_service_method (id, user_id, method, updated_at)
                VALUES (gen_random_uuid(), @userId, @method, now())",
                new { userId, method }, _tx);
        }
    }

    public async Task ReplaceAreasAsync(Guid userId, List<ServiceAreaDto> areas, CancellationToken ct)
    {
        await _conn.ExecuteAsync("DELETE FROM boss_service_area WHERE user_id=@userId", new { userId }, _tx);

        foreach (var area in areas)
        {
            await _conn.ExecuteAsync(@"
                INSERT INTO boss_service_area (id, user_id, city_id, district_id, created_at)
                VALUES (gen_random_uuid(), @userId, @CityId, @DistrictId, now())",
                new { userId, area.CityId, area.DistrictId }, _tx);
        }
    }

    public async Task ReplaceAddressesAsync(Guid userId, List<ServiceAddressDto> addresses, CancellationToken ct)
    {
        await _conn.ExecuteAsync("DELETE FROM boss_service_address WHERE user_id=@userId", new { userId }, _tx);

        foreach (var addr in addresses)
        {
            await _conn.ExecuteAsync(@"
                INSERT INTO boss_service_address (id, user_id, city_id, district_id, street, address_no, created_at, updated_at)
                VALUES (gen_random_uuid(), @userId, @CityId, @DistrictId, @Street, @AddressNo, now(), now())",
                new { userId, addr.CityId, addr.DistrictId, addr.Street, addr.AddressNo }, _tx);
        }
    }


}
