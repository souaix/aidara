// Infrastructure/Persistence/Postgres/UserRepo.cs
using Backend.Application.Ports;
using Backend.Domain.Entities;
using Dapper;
using Npgsql;

namespace Backend.Infrastructure.Persistence.Postgres;

public sealed class UserRepo : IUserRepo
{
    public async Task<User?> GetByEmailAsync(IUowContext uow, string email, CancellationToken ct)
    {
        const string sql = """
            SELECT user_id     AS UserId,
                   email,
                   display_name AS DisplayName,
                   boss_name    AS BossName,
                   avatar_url   AS AvatarUrl,
                   phone,
                   gender,
                   birthdate,
                   is_active    AS IsActive,
                   last_seen_at AS LastSeenAt,
                   created_at   AS CreatedAt,
                   updated_at   AS UpdatedAt
            FROM user_basis
            WHERE email = @email
            LIMIT 1;
            """;

        var conn = (NpgsqlConnection)uow.Connection;
        var tx = (NpgsqlTransaction?)uow.Transaction;

        return await conn.QueryFirstOrDefaultAsync<User>(
            new CommandDefinition(sql, new { email }, tx, cancellationToken: ct));
    }

    public async Task<User> InsertAsync(IUowContext uow, User user, CancellationToken ct)
    {
        const string sql = """
            INSERT INTO user_basis
              (user_id, email, display_name, boss_name, avatar_url, is_active, created_at, updated_at)
            VALUES
              (@UserId, @Email, @DisplayName, @BossName, @AvatarUrl, @IsActive, @CreatedAt, @UpdatedAt)
            RETURNING user_id     AS UserId,
                      email,
                      display_name AS DisplayName,
                      boss_name    AS BossName,
                      avatar_url   AS AvatarUrl,
                      phone,
                      gender,
                      birthdate,
                      is_active    AS IsActive,
                      last_seen_at AS LastSeenAt,
                      created_at   AS CreatedAt,
                      updated_at   AS UpdatedAt;
            """;

        var conn = (NpgsqlConnection)uow.Connection;
        var tx = (NpgsqlTransaction?)uow.Transaction;

        return await conn.QuerySingleAsync<User>(
            new CommandDefinition(sql, user, tx, cancellationToken: ct));
    }

    public async Task<User?> GetUserProfileAsync(IUowContext uow, Guid userId, CancellationToken ct)
    {
        const string sql = """
            SELECT user_id     AS UserId,
                   email,
                   display_name AS DisplayName,
                   boss_name    AS BossName,
                   avatar_url   AS AvatarUrl,
                   phone,
                   gender,
                   birthdate,
                   is_active    AS IsActive,
                   last_seen_at AS LastSeenAt,
                   created_at   AS CreatedAt,
                   updated_at   AS UpdatedAt
            FROM user_basis
            WHERE user_id = @userId
            LIMIT 1;
            """;

        var conn = (NpgsqlConnection)uow.Connection;
        var tx = (NpgsqlTransaction?)uow.Transaction;

        return await conn.QueryFirstOrDefaultAsync<User>(
            new CommandDefinition(sql, new { userId }, tx, cancellationToken: ct));
    }

    public async Task TouchLastSeenAsync(IUowContext uow, Guid userId, CancellationToken ct)
    {
        const string sql = """
            UPDATE user_basis
               SET last_seen_at = now(),
                   updated_at   = now()
             WHERE user_id = @userId;
            """;

        var conn = (NpgsqlConnection)uow.Connection;
        var tx = (NpgsqlTransaction?)uow.Transaction;

        await conn.ExecuteAsync(
            new CommandDefinition(sql, new { userId }, tx, cancellationToken: ct));
    }
}
