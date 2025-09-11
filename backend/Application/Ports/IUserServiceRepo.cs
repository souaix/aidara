using Backend.Application.ViewModels.Services;

public interface IUserServiceRepo
{
    Task<List<UserServiceItemVm>> GetUserServicesAsync(Guid userId, CancellationToken ct);
    Task UpdateUserServicesAsync(Guid userId, List<Guid> itemIds, CancellationToken ct);
}
