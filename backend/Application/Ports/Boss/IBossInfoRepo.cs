// Application/Ports/IBossInfoRepo.cs
using System.Data;
using Backend.Application.ViewModels.Services;

namespace Backend.Application.Ports;

public interface IBossInfoRepo
{
    /// <summary>
    /// 取代老闆的服務項目（完整覆蓋）
    /// </summary>
    Task ReplaceItemsAsync(IDbConnection conn, IDbTransaction? tx, Guid userId, List<ItemPriceRangeDto> items, CancellationToken ct);

    /// <summary>
    /// 取代老闆的服務方式（完整覆蓋）
    /// </summary>
    Task ReplaceMethodsAsync(IDbConnection conn, IDbTransaction? tx, Guid userId, List<string> methods, CancellationToken ct);

    /// <summary>
    /// 取代老闆的服務區域（完整覆蓋）
    /// </summary>
    Task ReplaceAreasAsync(IDbConnection conn, IDbTransaction? tx, Guid userId, List<ServiceAreaDto> areas, CancellationToken ct);

    /// <summary>
    /// 取代老闆的服務地址（完整覆蓋）
    /// </summary>
    Task ReplaceAddressesAsync(IDbConnection conn, IDbTransaction? tx, Guid userId, List<ServiceAddressDto> addresses, CancellationToken ct);
}
