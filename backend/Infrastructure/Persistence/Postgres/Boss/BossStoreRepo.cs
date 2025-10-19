// Infrastructure/Persistence/Postgres/BossStoreRepo.cs
using System.Data;
using Backend.Application.Ports;
using Backend.Application.ViewModels.Services;
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
}
