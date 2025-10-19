// Application/Ports/ICustomerServiceQuestionnaireRepo.cs
using System.Data;
using Backend.Application.ViewModels.Services;

namespace Backend.Application.Ports;

public interface ICustomerServiceQuestionnaireRepo
{
    /// <summary>
    /// 提交顧客服務問卷（覆蓋舊資料）
    /// </summary>
    Task SubmitAsync(IDbConnection conn, IDbTransaction? tx, Guid userId, CustomerServiceQuestionnaireDto dto, CancellationToken ct);
}
