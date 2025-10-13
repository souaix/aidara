using Backend.Application.Contracts.Users;
using Backend.Application.Ports;
using Backend.Domain.Entities;
using Dapper;
using Npgsql;

namespace Backend.Infrastructure.Persistence.Postgres;

public sealed class UserRepo : IUserRepo
{
    private readonly NpgsqlConnection _conn;
    private readonly NpgsqlTransaction? _tx;

    public UserRepo(NpgsqlConnection conn, NpgsqlTransaction? tx)
    {
        _conn = conn;
        _tx = tx;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct)
    {
        const string sql = """
            select user_id   as UserId,
                   email,
                   display_name as DisplayName,
                   boss_name    as BossName,
                   avatar_url   as AvatarUrl,
                   phone,
                   gender,
                   birthdate,
                   is_active    as IsActive,
                   last_seen_at as LastSeenAt,
                   created_at   as CreatedAt,
                   updated_at   as UpdatedAt
            from user_basis
            where email = @email
            limit 1;
            """;
        return await _conn.QueryFirstOrDefaultAsync<User>(
            new CommandDefinition(sql, new { email }, _tx, cancellationToken: ct));
    }

    public async Task<User> InsertAsync(User user, CancellationToken ct)
    {
        const string sql = """
            insert into user_basis
              (user_id, email, display_name, boss_name, avatar_url, is_active, created_at, updated_at)
            values
              (@UserId, @Email, @DisplayName, @BossName, @AvatarUrl, @IsActive, @CreatedAt, @UpdatedAt)
            returning user_id   as UserId,
                      email,
                      display_name as DisplayName,
                      boss_name    as BossName,
                      avatar_url   as AvatarUrl,
                      phone,
                      gender,
                      birthdate,
                      is_active    as IsActive,
                      last_seen_at as LastSeenAt,
                      created_at   as CreatedAt,
                      updated_at   as UpdatedAt;
            """;
        return await _conn.QuerySingleAsync<User>(
            new CommandDefinition(sql, user, _tx, cancellationToken: ct));
    }

    public async Task<User?> GetUserProfileAsync(Guid userId, CancellationToken ct)
    {
        const string sql = """
            SELECT 
                user_id       AS UserId,
                email,
                display_name  AS DisplayName,
                boss_name     AS BossName,
                avatar_url    AS AvatarUrl,
                phone,
                gender,
                birthdate,
                is_active     AS IsActive,
                last_seen_at  AS LastSeenAt,
                created_at    AS CreatedAt,
                updated_at    AS UpdatedAt
            FROM user_basis
            WHERE user_id = @userId
            LIMIT 1;
            """;

        return await _conn.QueryFirstOrDefaultAsync<User>(
            new CommandDefinition(sql, new { userId }, _tx, cancellationToken: ct));
    }

    // ✅ 取得主要角色（含名稱）
    public async Task<UserRoleDto?> GetUserRoleAsync(Guid userId, CancellationToken ct)
    {
        const string sql = """
            SELECT 
                ur.role_id AS RoleId,
                COALESCE(rb.role_name, ur.role_id) AS RoleName
            FROM user_role ur
            LEFT JOIN role_basis rb ON rb.role_id = ur.role_id
            WHERE ur.user_id = @userId
            ORDER BY ur.updatedate DESC
            LIMIT 1;
            """;

        return await _conn.QueryFirstOrDefaultAsync<UserRoleDto>(
            new CommandDefinition(sql, new { userId }, _tx, cancellationToken: ct));
    }

    // ✅ 取得所有角色（含 role_name）
    public async Task<List<UserRoleDto>> GetUserRolesAsync(Guid userId, CancellationToken ct)
    {
        const string sql = """
            SELECT 
                ur.user_id      AS UserId,
                ur.role_id      AS RoleId,
                COALESCE(rb.role_name, ur.role_id) AS RoleName,
                ur.createdate   AS CreateDate,
                ur.updatedate   AS UpdateDate,
                ur.expiredate   AS ExpireDate
            FROM user_role ur
            LEFT JOIN role_basis rb ON rb.role_id = ur.role_id
            WHERE ur.user_id = @userId;
            """;

        var rows = await _conn.QueryAsync<UserRoleDto>(
            new CommandDefinition(sql, new { userId }, _tx, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task AddUserRoleAsync(Guid userId, string roleId, DateTime? expireDate, CancellationToken ct)
    {
        const string sql = """
            INSERT INTO user_role (id, user_id, role_id, createdate, updatedate, expiredate)
            VALUES (gen_random_uuid(), @userId, @roleId, now(), now(), @expireDate)
            ON CONFLICT (user_id, role_id)
            DO UPDATE SET updatedate = now(), expiredate = @expireDate;
            """;
        await _conn.ExecuteAsync(new CommandDefinition(sql, new { userId, roleId, expireDate }, _tx, cancellationToken: ct));
    }

    public async Task RemoveUserRoleAsync(Guid userId, string roleId, CancellationToken ct)
    {
        const string sql = "DELETE FROM user_role WHERE user_id = @userId AND role_id = @roleId;";
        await _conn.ExecuteAsync(new CommandDefinition(sql, new { userId, roleId }, _tx, cancellationToken: ct));
    }

    public async Task TouchLastSeenAsync(Guid userId, CancellationToken ct)
    {
        const string sql = "UPDATE user_basis SET last_seen_at = now(), updated_at = now() WHERE user_id = @userId;";
        await _conn.ExecuteAsync(new CommandDefinition(sql, new { userId }, _tx, cancellationToken: ct));
    }
}
