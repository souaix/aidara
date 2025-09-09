using Backend.Application.Ports;
using Backend.Domain.Entities;
using Dapper;
using Npgsql;

namespace Backend.Infrastructure.Persistence.Postgres;

public sealed class UserRepo : IUserRepo
{
    private readonly NpgsqlConnection _conn;
    private readonly NpgsqlTransaction? _tx;

    // 由 UnitOfWork 建立同一條連線與交易後注入
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
        return await _conn.QueryFirstOrDefaultAsync<User>(new CommandDefinition(sql, new { email }, _tx, cancellationToken: ct));
		//return await _conn.QueryFirstOrDefaultAsync<User>(new CommandDefinition(sql, new { email }, cancellationToken: ct));
	}

    public async Task<User> InsertAsync(User user, CancellationToken ct)
    {
        const string sql = """
            insert into user_basis
              (user_id, email, display_name, avatar_url, is_active, created_at, updated_at)
            values
              (@UserId, @Email, @DisplayName, @AvatarUrl, @IsActive, @CreatedAt, @UpdatedAt)
            returning user_id   as UserId,
                      email,
                      display_name as DisplayName,
                      avatar_url   as AvatarUrl,
                      phone,
                      gender,
                      birthdate,
                      is_active    as IsActive,
                      last_seen_at as LastSeenAt,
                      created_at   as CreatedAt,
                      updated_at   as UpdatedAt;
            """;
        return await _conn.QuerySingleAsync<User>(new CommandDefinition(sql, user, _tx, cancellationToken: ct));
    }

    public async Task TouchLastSeenAsync(Guid userId, CancellationToken ct)
    {
        const string sql = "update user_basis set last_seen_at = now(), updated_at = now() where user_id = @userId;";
        await _conn.ExecuteAsync(new CommandDefinition(sql, new { userId }, _tx, cancellationToken: ct));
    }
}
