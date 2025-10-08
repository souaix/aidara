// Application/Ports/IServiceRepo.cs
using Backend.Application.ViewModels.Services;

public interface IServiceRepo
{
    Task<List<ServiceCategoryVm>> GetAllCategoriesAsync(string lang, CancellationToken ct);
}
