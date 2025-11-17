// Infrastructure/Persistence/Postgres/BossStoreRepo.cs
using System.Data;
using Backend.Application.Ports;
using Backend.Application.ViewModels.Services;
using Backend.Application.ViewModels.Boss;
using Dapper;

namespace Backend.Infrastructure.Persistence.Postgres;

public sealed class BossStoreRepo : IBossStoreRepo
{
    public async Task<List<StoreVm>> GetStoresByRegionAsync(
        IDbConnection conn,
        IDbTransaction? tx,
        Guid itemId,
        string cityId,
        string? districtId,
        CancellationToken ct)
    {
        const string sql = """
            SELECT
                u.user_id                    AS UserId,
                COALESCE(u.display_name,'')  AS DisplayName,
                COALESCE(u.avatar_url,'')    AS AvatarUrl,
                i.item_id                    AS ItemId,
                i.name                       AS ItemName,
                COALESCE(m.methods, ARRAY[]::text[]) AS Methods,
                COALESCE(s.avg_score, 0)::numeric(4,2) AS AvgScore,
                loc.city_id                  AS CityId,
                COALESCE(loc.district_id,'') AS DistrictId
            FROM boss_service_item bsi
            JOIN service_item   i   ON bsi.item_id = i.item_id
            JOIN user_basis     u   ON bsi.user_id = u.user_id
            JOIN user_store     loc ON loc.user_id = u.user_id
            LEFT JOIN (
                SELECT user_id, array_agg(method) AS methods
                FROM boss_service_method
                GROUP BY user_id
            ) m ON m.user_id = u.user_id
            LEFT JOIN (
                SELECT ratee_id AS user_id, AVG(score) AS avg_score
                FROM reviews
                GROUP BY ratee_id
            ) s ON s.user_id = u.user_id
            WHERE bsi.item_id = @ItemId
              AND loc.city_id = @CityId
              AND (NULLIF(@DistrictId,'') IS NULL OR loc.district_id = @DistrictId)
            ORDER BY AvgScore DESC NULLS LAST, DisplayName;
        """;

        var rows = await conn.QueryAsync<StoreVm>(
            new CommandDefinition(sql, new { ItemId = itemId, CityId = cityId, DistrictId = districtId }, tx, cancellationToken: ct));

        return rows.ToList();
    }

    public async Task<StoreDetailVm?> GetStoreDetailAsync(IDbConnection conn, IDbTransaction? tx, Guid userId, CancellationToken ct)
    {
        const string baseSql = """
            SELECT 
                ub.user_id                   AS UserId,
                COALESCE(ub.display_name,'') AS DisplayName,
                COALESCE(ub.avatar_url,'')   AS AvatarUrl,
                COALESCE(sc.content_html,'') AS ContentHtml
            FROM user_basis ub
            LEFT JOIN store_content sc ON sc.user_id = ub.user_id
            WHERE ub.user_id = @UserId;
        """;

        const string qaSql = """
            SELECT 
                q.title       AS Question,
                a.answer_text AS Answer
            FROM boss_service_question q
            JOIN boss_service_question_answer a ON a.question_id = q.id
            WHERE a.user_id = @UserId
            ORDER BY q.sort_order;
        """;

        var detail = await conn.QueryFirstOrDefaultAsync<StoreDetailVm>(
            new CommandDefinition(baseSql, new { UserId = userId }, tx, cancellationToken: ct));

        if (detail == null)
            return null;

        var qa = await conn.QueryAsync<QuestionAnswerVm>(
            new CommandDefinition(qaSql, new { UserId = userId }, tx, cancellationToken: ct));

        detail.Questionnaire = qa.ToList();
        return detail;
    }

    public async Task<List<BossServiceFullVm>> GetBossServicesAsync(
        IDbConnection conn, IDbTransaction? tx, Guid userId, CancellationToken ct)
    {
        const string sql = @"
        SELECT 
            i.id                AS Id,
            i.user_id           AS UserId,
            i.item_id           AS ItemId,
            i.min_price         AS MinPrice,
            i.max_price         AS MaxPrice,
            i.created_at        AS CreatedAt,

            -- 服務分類
            vh.category_name    AS CategoryName,
            vh.subcategory_name AS SubcategoryName,
            vh.item_name        AS ItemName,

            -- 方法
            m.method            AS Method,

            -- 服務範圍
            a.city_id           AS CityId,
            lc.city_name        AS CityName,
            a.district_id       AS DistrictId,
            ld.district_name    AS DistrictName,
            lp.locality         AS Locality,
            a.note              AS AreaNote,

            -- 服務地址
            ad.city_id          AS AddrCityId,
            lc2.city_name       AS AddrCityName,
            ad.district_id      AS AddrDistrictId,
            ld2.district_name   AS AddrDistrictName,
            ad.street           AS Street,
            ad.address_no       AS AddressNo,
            ad.phone            AS Phone,
            ad.contact_name     AS ContactName,
            ad.lat              AS Lat,
            ad.lng              AS Lng
        FROM boss_service_item i
        JOIN vw_service_hierarchy_zh vh ON vh.item_id = i.item_id
        LEFT JOIN boss_service_method   m  ON m.user_id = i.user_id
        LEFT JOIN boss_service_area     a  ON a.user_id = i.user_id
        LEFT JOIN location_city         lc ON lc.city_id = a.city_id
        LEFT JOIN location_district     ld ON ld.district_id = a.district_id
        LEFT JOIN location_postal       lp ON lp.postal_id = a.postal_id
        LEFT JOIN boss_service_address  ad ON ad.user_id = i.user_id
        LEFT JOIN location_city         lc2 ON lc2.city_id = ad.city_id
        LEFT JOIN location_district     ld2 ON ld2.district_id = ad.district_id
        WHERE i.user_id = @UserId
        ORDER BY i.created_at DESC;
    ";

        var dict = new Dictionary<Guid, BossServiceFullVm>();

        var rows = await conn.QueryAsync<BossServiceJoinRow>(
            new CommandDefinition(sql, new { UserId = userId }, tx, cancellationToken: ct));

        foreach (var r in rows)
        {
            if (!dict.TryGetValue(r.Id, out var vm))
            {
                vm = new BossServiceFullVm
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    ItemId = r.ItemId,
                    CategoryName = r.CategoryName,
                    SubcategoryName = r.SubcategoryName,
                    ItemName = r.ItemName,
                    MinPrice = r.MinPrice,
                    MaxPrice = r.MaxPrice,
                    CreatedAt = r.CreatedAt,
                    Methods = new List<string>(),
                    Areas = new List<BossServiceAreaVm>(),
                    Addresses = new List<BossServiceAddressVm>()
                };
                dict[r.Id] = vm;
            }

            if (!string.IsNullOrWhiteSpace(r.Method) && !vm.Methods.Contains(r.Method))
                vm.Methods.Add(r.Method);

            if (r.CityId > 0 && r.DistrictId > 0 &&
                !vm.Areas.Any(a => a.CityId == r.CityId && a.DistrictId == r.DistrictId))
                vm.Areas.Add(new BossServiceAreaVm
                {
                    CityId = r.CityId,
                    DistrictId = r.DistrictId,
                    CityName = r.CityName,
                    DistrictName = r.DistrictName,
                    Locality = r.Locality,
                    Note = r.AreaNote
                });

            if (r.AddrCityId > 0 && r.AddrDistrictId > 0 &&
                !vm.Addresses.Any(a => a.CityId == r.AddrCityId && a.DistrictId == r.AddrDistrictId && a.Street == r.Street))
                vm.Addresses.Add(new BossServiceAddressVm
                {
                    CityId = r.AddrCityId,
                    DistrictId = r.AddrDistrictId,
                    CityName = r.AddrCityName,
                    DistrictName = r.AddrDistrictName,
                    Street = r.Street,
                    AddressNo = r.AddressNo,
                    Phone = r.Phone,
                    ContactName = r.ContactName,
                    Lat = r.Lat,
                    Lng = r.Lng
                });
        }

        return dict.Values.ToList();
    }
}
