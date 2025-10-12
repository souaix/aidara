using Backend.Application.Ports;
using Backend.Application.ViewModels.Users;
using Dapper;
using Npgsql;
using System.Data;

public class UserRoleRepo : IUserRoleRepo
{
    private readonly NpgsqlConnection _conn;
    private readonly IDbTransaction? _tx;

    public UserRoleRepo(NpgsqlConnection conn, IDbTransaction? tx = null)
    {
        _conn = conn;
        _tx = tx;
    }

    public async Task<List<UserRoleDto>> GetUserRolesAsync(Guid userId, CancellationToken ct)
    {
        const string sql = @"
                    SELECT user_id AS UserId,
                           role_id AS RoleId,
                           createdate AS CreateDate,
                           updatedate AS UpdateDate,
                           expiredate AS ExpireDate
                    FROM user_role
                    WHERE user_id = @userId;";
        var rows = await _conn.QueryAsync<UserRoleDto>(sql, new { userId }, _tx);
        return rows.ToList();
    }

    public async Task AddUserRoleAsync(Guid userId, string roleId, DateTime? expireDate, CancellationToken ct)
    {
        const string sql = @"
            INSERT INTO user_role (id, user_id, role_id, createdate, updatedate, expiredate)
            VALUES (gen_random_uuid(), @userId, @roleId, now(), now(), @expireDate)
            ON CONFLICT (user_id, role_id)
            DO UPDATE SET updatedate = now(), expiredate = @expireDate;
        ";
        await _conn.ExecuteAsync(sql, new { userId, roleId, expireDate }, _tx);
    }

    public async Task RemoveUserRoleAsync(Guid userId, string roleId, CancellationToken ct)
    {
        const string sql = "DELETE FROM user_role WHERE user_id = @userId AND role_id = @roleId;";
        await _conn.ExecuteAsync(sql, new { userId, roleId }, _tx);
    }
}
