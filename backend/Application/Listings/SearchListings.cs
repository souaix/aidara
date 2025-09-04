// /Backend.Application/Listings/SearchListings.cs
using Backend.Application.Ports;
using Backend.Domain.Geo;
using Backend.Domain.Listings;

namespace Backend.Application.Listings;

public sealed class SearchListings
{
    private readonly IListingRepo _repo;

    public SearchListings(IListingRepo repo) => _repo = repo;

    /// <summary>
    /// 地圖視窗 + 篩選條件的搜尋用例
    /// </summary>
    public Task<(IReadOnlyList<Listing> Items, int Total)> HandleAsync(
        BBox bbox,
        decimal? minUnit,
        decimal? maxUnit,
        int? minRooms,
        int? maxRooms,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        // 基本防呆（避免 Controller 忘了檢查）
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 50;
        if (pageSize > 200) pageSize = 200;

        return _repo.SearchAsync(bbox, minUnit, maxUnit, minRooms, maxRooms, page, pageSize, ct);
    }
}
