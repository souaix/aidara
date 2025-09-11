using Backend.Application.Ports;
using Backend.Application.ViewModels.Services;
using Dapper;
using Npgsql;

public class UserServiceRepo : IUserServiceRepo
{
    private readonly NpgsqlDataSource _ds;

    public UserServiceRepo(NpgsqlDataSource ds)
    {
        _ds = ds;
    }

    public async Task<List<UserServiceItemVm>> GetUserServicesAsync(Guid userId, CancellationToken ct)
    {
        const string sql = @"
            SELECT usi.item_id, i.name AS item_name,
                   s.subcategory_id, s.name AS subcategory_name,
                   c.category_id, c.name AS category_name
            FROM user_service_item usi
            JOIN service_item i ON usi.item_id = i.item_id
            JOIN service_subcategory s ON i.subcategory_id = s.subcategory_id
            JOIN service_category c ON s.category_id = c.category_id
            WHERE usi.user_id = @userId
            ORDER BY c.sort_order, s.sort_order, i.sort_order;
        ";

        using var conn = await _ds.OpenConnectionAsync(ct);
        var rows = await conn.QueryAsync<UserServiceItemVm>(sql, new { userId });
        return rows.ToList();
    }

    public async Task UpdateUserServicesAsync(Guid userId, List<Guid> itemIds, CancellationToken ct)
    {
        using var conn = await _ds.OpenConnectionAsync(ct);
        using var tx = await conn.BeginTransactionAsync(ct);

        try
        {
            // 先清空舊資料
            await conn.ExecuteAsync(
                "DELETE FROM user_service_item WHERE user_id = @userId",
                new { userId }, tx);

            // 再批次插入新勾選的項目
            foreach (var itemId in itemIds)
            {
                await conn.ExecuteAsync(@"
                    INSERT INTO user_service_item (id, user_id, item_id, created_at)
                    VALUES (gen_random_uuid(), @userId, @itemId, now())
                ", new { userId, itemId }, tx);
            }

            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}
