// Application/Ports/IUserRepo.cs
using Backend.Domain.Entities;

namespace Backend.Application.Ports;

public interface IUserRepo
{
    Task<User?> GetByEmailAsync(IUowContext uow, string email, CancellationToken ct);
    Task<User> InsertAsync(IUowContext uow, User user, CancellationToken ct);
    Task<User?> GetUserProfileAsync(IUowContext uow, Guid userId, CancellationToken ct);
    Task TouchLastSeenAsync(IUowContext uow, Guid userId, CancellationToken ct);
}
