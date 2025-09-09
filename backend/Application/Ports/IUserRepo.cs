using Backend.Domain.Entities;

namespace Backend.Application.Ports;

public interface IUserRepo
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct);
    Task<User> InsertAsync(User user, CancellationToken ct);
    Task TouchLastSeenAsync(Guid userId, CancellationToken ct);
}
