// Infrastructure/Persistence/Postgres/UserRoleRepo.cs
using System.Data;
using Backend.Application.Contracts.User;
using Backend.Application.Ports;
using Dapper;

namespace Backend.Infrastructure.Persistence.Postgres;

public sealed class UserRoleRepo : IUserRoleRepo
{
    public async Task<List<UserRoleDto>> GetUserRolesAsync(IDbConnection conn, IDbTransaction? tx, Guid userId, CancellationToken ct)
    {
        const string sql = """
            SELECT user_id    AS UserId,
                   role_id    AS RoleId,
                   createdate AS CreateDate,
                   updatedate AS UpdateDate,
                   expiredate AS ExpireDate
            FROM user_role
            WHERE user_id = @userId;
        """;

        var rows = await conn.QueryAsync<UserRoleDto>(
            new CommandDefinition(sql, new { userId }, tx, cancellationToken: ct));

        return rows.ToList();
    }

    public async Task AddUserRoleAsync(IDbConnection conn, IDbTransaction? tx, Guid userId, string roleId, DateTime? expireDate, CancellationToken ct)
    {
        const string sql = """
            INSERT INTO user_role (id, user_id, role_id, createdate, updatedate, expiredate)
            VALUES (gen_random_uuid(), @userId, @roleId, now(), now(), @expireDate)
            ON CONFLICT (user_id, role_id)
            DO UPDATE SET updatedate = now(), expiredate = @expireDate;
        """;

        await conn.ExecuteAsync(
            new CommandDefinition(sql, new { userId, roleId, expireDate }, tx, cancellationToken: ct));
    }

    public async Task RemoveUserRoleAsync(IDbConnection conn, IDbTransaction? tx, Guid userId, string roleId, CancellationToken ct)
    {
        const string sql = """
            DELETE FROM user_role 
            WHERE user_id = @userId AND role_id = @roleId;
        """;

        await conn.ExecuteAsync(
            new CommandDefinition(sql, new { userId, roleId }, tx, cancellationToken: ct));
    }

    public async Task<List<UserRoleWithNameDto>> GetUserRolesWithNameAsync(
        IDbConnection conn, IDbTransaction? tx, Guid userId, CancellationToken ct)
    {
        const string sql = @"
        SELECT a.user_id as UserOd, a.role_id as RoleId, b.role_name as RoleName
        FROM user_role a
        LEFT JOIN role_basis b ON a.role_id = b.role_id
        WHERE a.user_id = @userId
          AND (a.expiredate IS NULL OR a.expiredate > NOW())
        ORDER BY b.role_name;
    ";

        var result = await conn.QueryAsync<UserRoleWithNameDto>(
            new CommandDefinition(sql, new { userId }, tx, cancellationToken: ct));
        return result.ToList();
    }

}
