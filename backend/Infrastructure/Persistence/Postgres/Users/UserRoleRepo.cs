using Backend.Application.Contracts.Users;
using Backend.Application.Ports;
using Dapper;
using Npgsql;

public class UserRoleRepo : IUserRoleRepo
{
    public async Task<List<UserRoleDto>> GetUserRolesAsync(IUowContext uow, Guid userId, CancellationToken ct)
    {
        const string sql = @"
            SELECT user_id AS UserId,
                   role_id AS RoleId,
                   createdate AS CreateDate,
                   updatedate AS UpdateDate,
                   expiredate AS ExpireDate
            FROM user_role
            WHERE user_id = @userId;
        ";

        var conn = (NpgsqlConnection)uow.Connection;
        var rows = await conn.QueryAsync<UserRoleDto>(
            sql,
            new { userId },
            (NpgsqlTransaction?)uow.Transaction
        );

        return rows.ToList();
    }

    public async Task AddUserRoleAsync(IUowContext uow, Guid userId, string roleId, DateTime? expireDate, CancellationToken ct)
    {
        const string sql = @"
            INSERT INTO user_role (id, user_id, role_id, createdate, updatedate, expiredate)
            VALUES (gen_random_uuid(), @userId, @roleId, now(), now(), @expireDate)
            ON CONFLICT (user_id, role_id)
            DO UPDATE SET updatedate = now(), expiredate = @expireDate;
        ";

        var conn = (NpgsqlConnection)uow.Connection;
        await conn.ExecuteAsync(
            sql,
            new { userId, roleId, expireDate },
            (NpgsqlTransaction?)uow.Transaction
        );
    }

    public async Task RemoveUserRoleAsync(IUowContext uow, Guid userId, string roleId, CancellationToken ct)
    {
        const string sql = "DELETE FROM user_role WHERE user_id = @userId AND role_id = @roleId;";

        var conn = (NpgsqlConnection)uow.Connection;
        await conn.ExecuteAsync(
            sql,
            new { userId, roleId },
            (NpgsqlTransaction?)uow.Transaction
        );
    }
}
