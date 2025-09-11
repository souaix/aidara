// Application/Ports/IServiceRepo.cs
using Backend.Application.ViewModels.Services;

public interface IServiceRepo
{
    Task<List<ServiceCategoryVm>> GetAllCategoriesAsync(CancellationToken ct);
}
