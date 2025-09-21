using Backend.Application.Ports;
using Backend.Application.ViewModels.Services;

namespace Infrastructure.Persistence.Mock
{
    public class MockBossStoreRepo : IBossStoreRepo
    {
        // 先準備 5 筆假資料
        private static readonly List<StoreVm> Stores = new()
        {
            new StoreVm(Guid.Parse("11111111-1111-1111-1111-111111111111"), "小王水電", "/images/default-avatar.png", Guid.Empty, "水電維修",
                new[] { "到府服務", "線上估價" }, 4.5m, "台北市", "文山區"),
            new StoreVm(Guid.Parse("22222222-2222-2222-2222-222222222222"), "阿美清潔", "/images/default-avatar.png", Guid.Empty, "清潔服務",
                new[] { "到府清潔" }, 4.8m, "台北市", "文山區"),
            new StoreVm(Guid.Parse("33333333-3333-3333-3333-333333333333"), "阿俊搬家", "/images/default-avatar.png", Guid.Empty, "搬家服務",
                new[] { "到府服務" }, 4.2m, "台北市", "文山區"),
            new StoreVm(Guid.Parse("44444444-4444-4444-4444-444444444444"), "小芳美容", "/images/default-avatar.png", Guid.Empty, "美容服務",
                new[] { "到店服務" }, 4.9m, "台中市", "文山區"),
            new StoreVm(Guid.Parse("55555555-5555-5555-5555-555555555555"), "阿宏家教", "/images/default-avatar.png", Guid.Empty, "家教服務",
                new[] { "線上教學", "到府教學" }, 4.7m, "台北市", "文山區")
        };

        public Task<List<StoreVm>> GetStoresByRegionAsync(Guid itemId, string cityId, string? districtId, CancellationToken ct)
        {
            // 篩選 cityId/districtId，itemId 只是帶入不檢查
            var result = Stores
                .Where(s => s.CityId == cityId && (string.IsNullOrEmpty(districtId) || s.DistrictId == districtId))
                .Select(s => s with { ItemId = itemId }) // 把傳進來的 itemId 填進去
                .ToList();

            return Task.FromResult(result);
        }

        public Task<StoreDetailVm?> GetStoreDetailAsync(Guid userId, CancellationToken ct)
        {
            var store = Stores.FirstOrDefault(s => s.UserId == userId);
            if (store == null) return Task.FromResult<StoreDetailVm?>(null);

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
    }
}
