// Infrastructure/Persistence/Postgres/CustomerServiceRequestRepo.cs
using System.Data;
using Backend.Application.Ports;
using Backend.Application.ViewModels.Services;
using Dapper;

namespace Backend.Infrastructure.Persistence.Postgres;

public sealed class CustomerServiceRequestRepo : ICustomerServiceRequestRepo
{
    public async Task InsertAsync(IDbConnection conn, IDbTransaction? tx, CustomerServiceRequestDto dto, CancellationToken ct)
    {
        var requestId = Guid.NewGuid();

        const string insertRequestSql = """
            INSERT INTO customer_service_request 
                (request_id, user_id, item_id, price_min, price_max, created_at)
            VALUES 
                (@requestId, @userId, @itemId, @priceMin, @priceMax, now());
        """;

        await conn.ExecuteAsync(
            new CommandDefinition(insertRequestSql, new
            {
                requestId,
                userId = dto.UserId,
                itemId = dto.ItemId,
                priceMin = dto.PriceMin,
                priceMax = dto.PriceMax
            }, tx, cancellationToken: ct));

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
