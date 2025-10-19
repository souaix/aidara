// Application/Ports/ICustomerServiceRequestRepo.cs
using System.Data;
using Backend.Application.ViewModels.Services;

namespace Backend.Application.Ports;

public interface ICustomerServiceRequestRepo
{
    /// <summary>
    /// 新增一筆顧客服務請求（含服務方式）
    /// </summary>
    Task InsertAsync(IDbConnection conn, IDbTransaction? tx, CustomerServiceRequestDto dto, CancellationToken ct);
}
