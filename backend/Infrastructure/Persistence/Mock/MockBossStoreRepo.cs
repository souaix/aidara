// Infrastructure/Persistence/Mock/MockBossStoreRepo.cs
using Backend.Application.Ports;
using Backend.Application.ViewModels.Boss;
using Backend.Application.ViewModels.Services;
using System.Data;

namespace Backend.Infrastructure.Persistence.Mock;

public sealed class MockBossStoreRepo : IBossStoreRepo
{
    // 模擬 5 筆假資料
    private static readonly List<StoreVm> Stores = new()
    {
        new StoreVm(Guid.Parse("4d0c960f-5fda-4605-9c5a-ca087fe7f714"), "小王水電", "/images/default-avatar.png", Guid.Empty, "水電維修",
            new[] { "到府服務", "線上估價" }, 4.5m, "台北市", "文山區"),
        new StoreVm(Guid.Parse("22222222-2222-2222-2222-222222222222"), "阿美清潔", "/images/default-avatar.png", Guid.Empty, "清潔服務",
            new[] { "到府清潔" }, 4.8m, "台北市", "文山區"),
        new StoreVm(Guid.Parse("33333333-3333-3333-3333-333333333333"), "阿俊搬家", "/images/default-avatar.png", Guid.Empty, "搬家服務",
            new[] { "到府服務" }, 4.2m, "台北市", "文山區"),
        new StoreVm(Guid.Parse("44444444-4444-4444-4444-444444444444"), "小芳美容", "/images/default-avatar.png", Guid.Empty, "美容服務",
            new[] { "到店服務" }, 4.9m, "台中市", "西區"),
        new StoreVm(Guid.Parse("55555555-5555-5555-5555-555555555555"), "阿宏家教", "/images/default-avatar.png", Guid.Empty, "家教服務",
            new[] { "線上教學", "到府教學" }, 4.7m, "台北市", "文山區")
    };

    public Task<List<StoreVm>> GetStoresByRegionAsync(
        IDbConnection conn,
        IDbTransaction? tx,
        Guid itemId,
        string cityId,
        string? districtId,
        CancellationToken ct)
    {
        // 模擬篩選 cityId/districtId（conn, tx 不使用）
        var result = Stores
            .Where(s => s.CityId == cityId && (string.IsNullOrEmpty(districtId) || s.DistrictId == districtId))
            .Select(s => s with { ItemId = itemId })
            .ToList();

        return Task.FromResult(result);
    }

    public Task<StoreDetailVm?> GetStoreDetailAsync(
        IDbConnection conn,
        IDbTransaction? tx,
        Guid userId,
        CancellationToken ct)
    {
        var store = Stores.FirstOrDefault(s => s.UserId == userId);
        if (store == null)
            return Task.FromResult<StoreDetailVm?>(null);

        var detail = new StoreDetailVm
        {
            UserId = store.UserId,
            DisplayName = store.DisplayName,
            AvatarUrl = store.AvatarUrl,
            ContentHtml = $"<p><strong>{store.DisplayName} 簡介：</strong>這是一家提供 {store.ItemName} 的優質商家。</p>",
            Questionnaire = new List<QuestionAnswerVm>
            {
                new("服務範圍", $"{store.CityId} {store.DistrictId}"),
                new("營業時間", "09:00 - 18:00"),
                new("是否提供發票", "是"),
                new("特色服務", string.Join("、", store.Methods)),
                new("平均評分", store.AvgScore.ToString("0.0"))
            }
        };

        return Task.FromResult<StoreDetailVm?>(detail);
    }

    public Task<List<BossServiceFullVm>> GetBossServicesAsync(IDbConnection conn, IDbTransaction? tx, Guid userId, CancellationToken ct)
    {
        // 模擬三個服務項目
        var list = new List<BossServiceFullVm>
        {
            new BossServiceFullVm
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ItemId = "interior001",
                MinPrice = 1500,
                MaxPrice = 5000,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                Methods = new List<string> { "到府服務", "線上諮詢" },
                Areas = new List<BossServiceAreaVm>
                {
                    new BossServiceAreaVm { CityId = 1, DistrictId = 101, Note = "台北市中山區" },
                    new BossServiceAreaVm { CityId = 1, DistrictId = 105, Note = "台北市松山區" }
                },
                Addresses = new List<BossServiceAddressVm>
                {
                    new BossServiceAddressVm {
                        CityId = 1, DistrictId = 101,
                        Street = "南京東路", AddressNo = "12號3樓",
                        Phone = "0912-345-678", ContactName = "林小姐",
                        Lat = 25.0478m, Lng = 121.5319m
                    }
                }
            },
            new BossServiceFullVm
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ItemId = "cleaning002",
                MinPrice = 800,
                MaxPrice = 2000,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                Methods = new List<string> { "包月清潔" },
                Areas = new List<BossServiceAreaVm>
                {
                    new BossServiceAreaVm { CityId = 2, DistrictId = 202, Note = "新北市板橋區" }
                },
                Addresses = new List<BossServiceAddressVm>
                {
                    new BossServiceAddressVm {
                        CityId = 2, DistrictId = 202,
                        Street = "文化路", AddressNo = "88號",
                        Phone = "0933-123-999", ContactName = "王先生",
                        Lat = 25.013m, Lng = 121.467m
                    }
                }
            }
        };

        return Task.FromResult(list);
    }
}
