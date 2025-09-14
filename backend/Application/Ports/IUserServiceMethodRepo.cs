using Backend.Application.ViewModels.Services;

public interface IUserServiceMethodRepo
{
    Task<List<UserServiceMethodVm>> GetUserMethodsAsync(Guid userId, CancellationToken ct);
    Task UpdateUserMethodsAsync(Guid userId, List<string> methods, CancellationToken ct);
}
