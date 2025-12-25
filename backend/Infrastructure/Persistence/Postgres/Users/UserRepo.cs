// Infrastructure/Persistence/Postgres/UserRepo.cs
using System.Data;
using Backend.Application.Ports;
using Backend.Domain.Entities;
using Dapper;

namespace Backend.Infrastructure.Persistence.Postgres;

public sealed class UserRepo : IUserRepo
{
    public async Task<User?> GetByEmailAsync(IDbConnection conn, IDbTransaction? tx, string email, CancellationToken ct)
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

        return await conn.QueryFirstOrDefaultAsync<User>(
            new CommandDefinition(sql, new { email }, tx, cancellationToken: ct));
    }

    public async Task<User> InsertAsync(IDbConnection conn, IDbTransaction? tx, User user, CancellationToken ct)
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

        return await conn.QuerySingleAsync<User>(
            new CommandDefinition(sql, user, tx, cancellationToken: ct));
    }

    public async Task<User> UpdateAsync(IDbConnection conn, IDbTransaction? tx, User user, CancellationToken ct)
    {
        const string sql = """
            UPDATE user_basis
            SET
                display_name = @DisplayName,
                avatar_url  = @AvatarUrl,
                phone       = @Phone,
                gender      = @Gender,
                birthdate   = @Birthdate,
                city        = @City,
                district    = @District,
                street      = @Street,
                address_no  = @AddressNo,
                is_active   = @IsActive,
                updated_at  = @UpdatedAt
            WHERE user_id = @UserId
            RETURNING
                user_id     AS UserId,
                email,
                display_name AS DisplayName,
                boss_name    AS BossName,
                avatar_url   AS AvatarUrl,
                phone,
                gender,
                birthdate,
                city,
                district,
                street,
                address_no,
                is_active    AS IsActive,
                last_seen_at AS LastSeenAt,
                created_at   AS CreatedAt,
                updated_at   AS UpdatedAt;
            """;

        return await conn.QuerySingleAsync<User>(
            new CommandDefinition(sql, user, tx, cancellationToken: ct));
    }

    public async Task<User?> GetUserProfileAsync(IDbConnection conn, IDbTransaction? tx, Guid userId, CancellationToken ct)
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
                   city,
                   district,
                   street,
                   address_no   AS AddressNo,
                   is_active    AS IsActive,
                   last_seen_at AS LastSeenAt,
                   created_at   AS CreatedAt,
                   updated_at   AS UpdatedAt
            FROM user_basis
            WHERE user_id = @userId
            LIMIT 1;
        """;

        return await conn.QueryFirstOrDefaultAsync<User>(
            new CommandDefinition(sql, new { userId }, tx, cancellationToken: ct));
    }

    public async Task TouchLastSeenAsync(IDbConnection conn, IDbTransaction? tx, Guid userId, CancellationToken ct)
    {
        const string sql = """
            UPDATE user_basis
               SET last_seen_at = now(),
                   updated_at   = now()
             WHERE user_id = @userId;
        """;

        await conn.ExecuteAsync(
            new CommandDefinition(sql, new { userId }, tx, cancellationToken: ct));
    }

    public async Task UpdateAvatarAsync(IDbConnection conn, IDbTransaction? tx, Guid userId, string relativePath, CancellationToken ct)
    {
        Console.WriteLine("WRRRRRRITE!!"+ relativePath);
        const string sql = """
            UPDATE user_basis
               SET avatar_url = @relativePath,
                   updated_at   = now()
             WHERE user_id = @userId;
        """;

        await conn.ExecuteAsync(
            new CommandDefinition(sql, new { userId = userId, relativePath = relativePath }, tx, cancellationToken: ct));
    }
}
