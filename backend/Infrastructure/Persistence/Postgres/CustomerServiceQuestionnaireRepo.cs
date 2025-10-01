using Dapper;
using Npgsql;

public class CustomerServiceQuestionnaireRepo : ICustomerServiceQuestionnaireRepo
{
    private readonly NpgsqlConnection _conn;
    private readonly NpgsqlTransaction _tx;

    public CustomerServiceQuestionnaireRepo(NpgsqlConnection conn, NpgsqlTransaction tx)
    {
        _conn = conn;
        _tx = tx;
    }

    public async Task SubmitAsync(Guid userId, CustomerServiceQuestionnaireDto dto, CancellationToken ct)
    {
        var requestId = Guid.NewGuid();
        await _conn.ExecuteAsync(@"
        INSERT INTO customer_service_request
          (request_id, user_id, item_id, price_min, price_max, created_at)
        VALUES (@requestId, @userId, @itemId, @priceMin, @priceMax, now())
        ON CONFLICT (user_id) DO UPDATE
        SET item_id   = EXCLUDED.item_id,
            price_min = EXCLUDED.price_min,
            price_max = EXCLUDED.price_max,
            updated_at= now()",
            new
            {
                requestId,
                userId,                // ← 從參數來，不是 dto
                itemId = dto.ItemId,
                priceMin = dto.PriceMin,
                priceMax = dto.PriceMax
            }, _tx);

        // methods 覆蓋：先刪後插
        await _conn.ExecuteAsync("DELETE FROM customer_service_method WHERE request_id = @requestId",
            new { requestId }, _tx);

        foreach (var m in dto.Methods ?? Enumerable.Empty<string>())
        {
            await _conn.ExecuteAsync(@"
            INSERT INTO customer_service_method (id, request_id, method)
            VALUES (gen_random_uuid(), @requestId, @method)",
                new { requestId, method = m }, _tx);
        }
    }

}
