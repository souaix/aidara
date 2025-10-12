using Backend.Application.Ports;
using Backend.Application.ViewModels.Users;
using Dapper;
using Npgsql;
using System.Data;

public class RoleBasisRepo : IRoleBasisRepo
{
    private readonly NpgsqlConnection _conn;
    private readonly IDbTransaction? _tx;

    public RoleBasisRepo(NpgsqlConnection conn, IDbTransaction? tx = null)
    {
        _conn = conn;
        _tx = tx;
    }

    public async Task<List<RoleBasisDto>> GetAllRolesAsync(CancellationToken ct)
    {
        const string sql = @"
            SELECT role_id AS RoleId,
                   role_name AS RoleName,
                   description AS Description,
                   is_active AS IsActive,
                   createdate AS CreateDate,
                   updatedate AS UpdateDate
            FROM role_basis
            ORDER BY role_id;
        ";
        var rows = await _conn.QueryAsync<RoleBasisDto>(sql, transaction: _tx);
        return rows.ToList();
    }

    public async Task<RoleBasisDto?> GetRoleAsync(string roleId, CancellationToken ct)
    {
        const string sql = @"
            SELECT role_id AS RoleId,
                   role_name AS RoleName,
                   description AS Description,
                   is_active AS IsActive,
                   createdate AS CreateDate,
                   updatedate AS UpdateDate
            FROM role_basis
            WHERE role_id = @roleId;
        ";
        return await _conn.QueryFirstOrDefaultAsync<RoleBasisDto>(sql, new { roleId }, _tx);
    }
}
