// Application/Ports/IBossStoreRepo.cs
using System.Data;
using Backend.Application.ViewModels.Services;

namespace Backend.Application.Ports;

public interface IBossStoreRepo
{
    /// <summary>
    /// 取得指定地區的店家清單
    /// </summary>
    Task<List<StoreVm>> GetStoresByRegionAsync(IDbConnection conn, IDbTransaction? tx, Guid itemId, string cityId, string? districtId, CancellationToken ct);

    /// <summary>
    /// 取得單一商家的詳細資訊 (問卷結果 + 自定義 Quill 內容)
    /// </summary>
    Task<StoreDetailVm?> GetStoreDetailAsync(IDbConnection conn, IDbTransaction? tx, Guid userId, CancellationToken ct);
}
