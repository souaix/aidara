using Dapper;
using Npgsql;
using System.Data;

namespace Backend.Infrastructure.Persistence.Postgres;

public sealed class PsqlRepository
{
    private readonly NpgsqlDataSource _ds;
    public PsqlRepository(NpgsqlDataSource ds) => _ds = ds;

    private async Task<NpgsqlConnection> GetConnAsync(CancellationToken ct = default)
    {
        return (NpgsqlConnection)await _ds.OpenConnectionAsync(ct);
    }

    // 查詢多筆
    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, CancellationToken ct = default)
    {
        await using var conn = await GetConnAsync(ct);
        return await conn.QueryAsync<T>(sql, param);
    }

    // 查詢單筆（找不到會回傳 default）
    public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, CancellationToken ct = default)
    {
        await using var conn = await GetConnAsync(ct);
        return await conn.QueryFirstOrDefaultAsync<T>(sql, param);
    }

    // 新增/更新/刪除（回傳受影響筆數）
    public async Task<int> ExecuteAsync(string sql, object? param = null, CancellationToken ct = default)
    {
        await using var conn = await GetConnAsync(ct);
        return await conn.ExecuteAsync(sql, param);
    }

    // 取回單一值（例如 count(*)）
    public async Task<T> ExecuteScalarAsync<T>(string sql, object? param = null, CancellationToken ct = default)
    {
        await using var conn = await GetConnAsync(ct);
        return await conn.ExecuteScalarAsync<T>(sql, param);
    }

    // 插入並回傳物件
    public async Task<T> InsertAndReturnAsync<T>(string sql, object? param = null, CancellationToken ct = default)
    {
        await using var conn = await GetConnAsync(ct);
        return await conn.QuerySingleAsync<T>(sql, param);
    }
}
