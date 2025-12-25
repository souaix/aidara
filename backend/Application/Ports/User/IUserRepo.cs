// Application/Ports/IUserRepo.cs
using Backend.Domain.Entities;
using System.Data;

namespace Backend.Application.Ports;

public interface IUserRepo
{
    Task<User?> GetByEmailAsync(IDbConnection conn, IDbTransaction? tx, string email, CancellationToken ct);
    Task<User> InsertAsync(IDbConnection conn, IDbTransaction? tx, User user, CancellationToken ct);
    Task<User> UpdateAsync(IDbConnection conn, IDbTransaction? tx, User user, CancellationToken ct);
    Task<User?> GetUserProfileAsync(IDbConnection conn, IDbTransaction? tx, Guid userId, CancellationToken ct);
    Task TouchLastSeenAsync(IDbConnection conn, IDbTransaction? tx, Guid userId, CancellationToken ct);
    Task UpdateAvatarAsync(IDbConnection conn, IDbTransaction? tx, Guid userId, string relativePath, CancellationToken ct);
}
