using System.Collections.Concurrent;
namespace Backend.Infrastructure.Persistence.Mock;
public class MockCustomerDemandRepo : ICustomerDemandRepo
{
    // 簡單的 in-memory store（RequestId 當主鍵）
    private static readonly ConcurrentDictionary<Guid, DemandVm> _store = new();

    static MockCustomerDemandRepo()
    {
        // 種子資料
        Seed("台北市", "家事清潔", 1000, 2000, new[] { "上門服務" });
        Seed("新北市", "兒童家教", 600, 1200, new[] { "固定據點", "線上服務" });
        Seed("高雄市", "電腦維修", 800, 1500, new[] { "上門服務" });
        Seed("台北市", "寵物美容", null, 1800, new[] { "固定據點" });
    }

    private static void Seed(string city, string itemName, int? min, int? max, string[] methods)
    {
        var id = Guid.NewGuid();
        _store[id] = new DemandVm(
            RequestId: id,
            City: city,
            ItemName: itemName,
            PriceMin: min,
            PriceMax: max,
            Methods: methods,
            CreatedAt: DateTime.UtcNow.AddMinutes(-Random.Shared.Next(0, 10_000))
        );
    }

    public Task<IReadOnlyList<CityCountVm>> GetCityCountsAsync(CancellationToken ct)
    {
        var result = _store.Values
            .GroupBy(d => d.City)
            .Select(g => new CityCountVm(g.Key, g.Count()))
            .OrderBy(c => c.City)
            .ToList()
            .AsReadOnly();

        return Task.FromResult<IReadOnlyList<CityCountVm>>(result);
    }

    public Task<IReadOnlyList<DemandVm>> GetDemandsByCityAsync(string city, CancellationToken ct)
    {
        var result = _store.Values
            .Where(d => string.Equals(d.City, city, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(d => d.CreatedAt)
            .ToList()
            .AsReadOnly();

        return Task.FromResult<IReadOnlyList<DemandVm>>(result);
    }

    // （選配）給你在跑 App 時動態加入 mock 需求單用
    public static void Upsert(Guid requestId, DemandVm vm) => _store[requestId] = vm;
}
