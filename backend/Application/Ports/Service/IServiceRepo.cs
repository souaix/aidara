// Application/Ports/IServiceRepo.cs
using System.Data;
using Backend.Application.ViewModels.Services;

namespace Backend.Application.Ports;

public interface IServiceRepo
{
    Task<List<ServiceCategoryVm>> GetAllCategoriesAsync(
        IDbConnection conn,
        IDbTransaction? tx,
        string lang,
        CancellationToken ct);
}
