// Application/Ports/IServiceStatRepo.cs
using System.Data;
using Backend.Application.ViewModels.Services;

namespace Backend.Application.Ports;

public interface IBossServiceStatRepo
{
    /// <summary>
    /// 取得某服務項目在各縣市區的店家數
    /// </summary>
    Task<List<ServiceStatVm>> GetServiceStatsAsync(IDbConnection conn, IDbTransaction? tx, Guid itemId, CancellationToken ct);
}
