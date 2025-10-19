// Infrastructure/Persistence/Postgres/CustomerServiceQuestionnaireRepo.cs
using System.Data;
using Backend.Application.Ports;
using Backend.Application.ViewModels.Services;
using Dapper;

namespace Backend.Infrastructure.Persistence.Postgres;

public sealed class CustomerServiceQuestionnaireRepo : ICustomerServiceQuestionnaireRepo
{
    public async Task SubmitAsync(IDbConnection conn, IDbTransaction? tx, Guid userId, CustomerServiceQuestionnaireDto dto, CancellationToken ct)
    {
        var requestId = Guid.NewGuid();

        const string insertRequestSql = """
            INSERT INTO customer_service_request
                (request_id, user_id, item_id, price_min, price_max, created_at)
            VALUES
                (@requestId, @userId, @itemId, @priceMin, @priceMax, now())
            ON CONFLICT (user_id) DO UPDATE
            SET item_id   = EXCLUDED.item_id,
                price_min = EXCLUDED.price_min,
                price_max = EXCLUDED.price_max,
                updated_at= now();
        """;

        await conn.ExecuteAsync(
            new CommandDefinition(insertRequestSql, new
            {
                requestId,
                userId,
                itemId = dto.ItemId,
                priceMin = dto.PriceMin,
                priceMax = dto.PriceMax
            }, tx, cancellationToken: ct));

        // 先刪除舊的服務方式
        const string deleteMethodsSql = "DELETE FROM customer_service_method WHERE request_id = @requestId;";
        await conn.ExecuteAsync(
            new CommandDefinition(deleteMethodsSql, new { requestId }, tx, cancellationToken: ct));

        // 重新插入新的服務方式
        if (dto.Methods is not null && dto.Methods.Any())
        {
            const string insertMethodSql = """
                INSERT INTO customer_service_method (id, request_id, method)
                VALUES (gen_random_uuid(), @requestId, @method);
            """;

            foreach (var method in dto.Methods)
            {
                await conn.ExecuteAsync(
                    new CommandDefinition(insertMethodSql, new { requestId, method }, tx, cancellationToken: ct));
            }
        }
    }
}
