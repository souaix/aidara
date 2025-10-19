// Infrastructure/Persistence/Postgres/RoleBasisRepo.cs
using System.Data;
using Backend.Application.Ports;
using Backend.Application.Contracts.User;
using Dapper;

namespace Backend.Infrastructure.Persistence.Postgres;

public sealed class RoleBasisRepo : IRoleBasisRepo
{
    public async Task<List<RoleBasisDto>> GetAllRolesAsync(IDbConnection conn, IDbTransaction? tx, CancellationToken ct)
    {
        const string sql = """
            SELECT role_id    AS RoleId,
                   role_name  AS RoleName,
                   description AS Description,
                   is_active  AS IsActive,
                   createdate AS CreateDate,
                   updatedate AS UpdateDate
            FROM role_basis
            ORDER BY role_id;
        """;

        var rows = await conn.QueryAsync<RoleBasisDto>(
            new CommandDefinition(sql, transaction: tx, cancellationToken: ct));

        return rows.ToList();
    }

    public async Task<RoleBasisDto?> GetRoleAsync(IDbConnection conn, IDbTransaction? tx, string roleId, CancellationToken ct)
    {
        const string sql = """
            SELECT role_id    AS RoleId,
                   role_name  AS RoleName,
                   description AS Description,
                   is_active  AS IsActive,
                   createdate AS CreateDate,
                   updatedate AS UpdateDate
            FROM role_basis
            WHERE role_id = @roleId;
        """;

        return await conn.QueryFirstOrDefaultAsync<RoleBasisDto>(
            new CommandDefinition(sql, new { roleId }, tx, cancellationToken: ct));
    }
}
