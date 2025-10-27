using Backend.Application.Ports;
using Dapper;
using Npgsql;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Backend.Infrastructure.Persistence.Postgres
{
    public class UserActiveModeRepo : IUserActiveModeRepo
    {
        private readonly NpgsqlDataSource _ds;
        public UserActiveModeRepo(NpgsqlDataSource ds) => _ds = ds;

        public async Task<string?> GetActiveModeAsync(IDbConnection conn, IDbTransaction? tx, Guid userId, CancellationToken ct)
        {
            const string sql = @"SELECT active_role FROM user_active_mode WHERE user_id = @userId;";
            return await conn.ExecuteScalarAsync<string?>(new CommandDefinition(sql, new { userId }, tx, cancellationToken: ct));
        }

        public async Task SetActiveModeAsync(IDbConnection conn, IDbTransaction? tx, Guid userId, string roleId, CancellationToken ct)
        {
            const string sql = @"
                INSERT INTO user_active_mode (user_id, active_role, updatedate)
                VALUES (@userId, @roleId, now())
                ON CONFLICT (user_id)
                DO UPDATE SET active_role = @roleId, updatedate = now();
            ";            
            await conn.ExecuteAsync(new CommandDefinition(sql, new { userId, roleId }, tx, cancellationToken: ct));
        }
    }
}
