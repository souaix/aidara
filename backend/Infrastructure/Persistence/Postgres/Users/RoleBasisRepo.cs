using Backend.Application.Ports;
using Backend.Application.Contracts.Users;
using Dapper;
using Npgsql;

public class RoleBasisRepo : IRoleBasisRepo
{
    public async Task<List<RoleBasisDto>> GetAllRolesAsync(IUowContext uow, CancellationToken ct)
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

        var conn = (NpgsqlConnection)uow.Connection;
        var rows = await conn.QueryAsync<RoleBasisDto>(
            sql,
            transaction: (NpgsqlTransaction?)uow.Transaction
        );

        return rows.ToList();
    }

    public async Task<RoleBasisDto?> GetRoleAsync(IUowContext uow, string roleId, CancellationToken ct)
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

        var conn = (NpgsqlConnection)uow.Connection;
        return await conn.QueryFirstOrDefaultAsync<RoleBasisDto>(
            sql,
            new { roleId },
            (NpgsqlTransaction?)uow.Transaction
        );
    }
}
