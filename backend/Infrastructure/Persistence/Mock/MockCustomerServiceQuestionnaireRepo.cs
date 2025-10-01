using Backend.Application.Ports;
using System.Collections.Concurrent;

public class MockCustomerServiceQuestionnaireRepo : ICustomerServiceQuestionnaireRepo
{
    private static readonly ConcurrentDictionary<Guid, CustomerServiceQuestionnaireDto> _store = new();

    public Task SubmitAsync(Guid userId, CustomerServiceQuestionnaireDto dto, CancellationToken ct)
    {
        // 模擬 UPSERT：直接覆蓋
        _store[userId] = dto;
        return Task.CompletedTask;
    }

    // 額外給測試用的取回方法
    public Task<CustomerServiceQuestionnaireDto?> GetAsync(Guid userId, CancellationToken ct)
    {
        _store.TryGetValue(userId, out var dto);
        return Task.FromResult(dto);
    }
}
