using Backend.Application.Ports;
using Backend.Application.ViewModels.Services;
using Dapper;
using Npgsql;

public class UserServiceMethodRepo : IUserServiceMethodRepo
{
    private readonly NpgsqlDataSource _ds;

    public UserServiceMethodRepo(NpgsqlDataSource ds)
    {
        _ds = ds;
    }

    public async Task<List<UserServiceMethodVm>> GetUserMethodsAsync(Guid userId, CancellationToken ct)
    {
        const string sql = @"
            SELECT id, user_id, method, updated_at
            FROM boss_service_method
            WHERE user_id = @userId
            ORDER BY updated_at DESC;
        ";

        using var conn = await _ds.OpenConnectionAsync(ct);
        var rows = await conn.QueryAsync<UserServiceMethodVm>(sql, new { userId });
        return rows.ToList();
    }

    public async Task UpdateUserMethodsAsync(Guid userId, List<string> methods, CancellationToken ct)
    {
        using var conn = await _ds.OpenConnectionAsync(ct);
        using var tx = await conn.BeginTransactionAsync(ct);

        try
        {
            // 先刪舊的
            await conn.ExecuteAsync(
                "DELETE FROM boss_service_method WHERE user_id = @userId",
                new { userId }, tx);

            // 插入新的
            foreach (var m in methods)
            {
                await conn.ExecuteAsync(@"
                    INSERT INTO boss_service_method (id, user_id, method, updated_at)
                    VALUES (gen_random_uuid(), @userId, @method, now())
                ", new { userId, method = m }, tx);
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
