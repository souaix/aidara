using Dapper;
using Npgsql;

public class CustomerServiceRequestRepo : ICustomerServiceRequestRepo
{
    private readonly NpgsqlConnection _conn;
    private readonly NpgsqlTransaction _tx;

    public CustomerServiceRequestRepo(NpgsqlConnection conn, NpgsqlTransaction tx)
    {
        _conn = conn;
        _tx = tx;
    }

    public async Task InsertAsync(CustomerServiceRequestDto dto, CancellationToken ct)
    {
        var requestId = Guid.NewGuid();

        await _conn.ExecuteAsync(@"
            INSERT INTO customer_service_request (request_id, user_id, item_id, price_min, price_max, created_at)
            VALUES (@requestId, @userId, @itemId, @priceMin, @priceMax, now())
        ", new
        {
            requestId,
            userId = dto.UserId,
            itemId = dto.ItemId,
            priceMin = dto.PriceMin,
            priceMax = dto.PriceMax
        }, _tx);

        foreach (var m in dto.Methods)
        {
            await _conn.ExecuteAsync(@"
                INSERT INTO customer_service_method (id, request_id, method)
                VALUES (gen_random_uuid(), @requestId, @method)
            ", new { requestId, method = m }, _tx);
        }
    }
}
