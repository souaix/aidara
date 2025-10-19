// Infrastructure/Persistence/Mock/MockCustomerServiceQuestionnaireRepo.cs
using System.Collections.Concurrent;
using System.Data;
using Backend.Application.Ports;
using Backend.Application.ViewModels.Services;

namespace Backend.Infrastructure.Persistence.Mock;

public sealed class MockCustomerServiceQuestionnaireRepo : ICustomerServiceQuestionnaireRepo
{
    private static readonly ConcurrentDictionary<Guid, CustomerServiceQuestionnaireDto> _store = new();

    public Task SubmitAsync(IDbConnection conn, IDbTransaction? tx, Guid userId, CustomerServiceQuestionnaireDto dto, CancellationToken ct)
    {
        // 模擬 UPSERT：直接覆蓋
        _store[userId] = dto;
        Console.WriteLine($"[Mock] CustomerServiceQuestionnaire Submit: UserId={userId}, ItemId={dto.ItemId}, PriceRange={dto.PriceMin}-{dto.PriceMax}");
        return Task.CompletedTask;
    }

    // 額外提供給測試檢查結果
    public Task<CustomerServiceQuestionnaireDto?> GetAsync(Guid userId, CancellationToken ct)
    {
        _store.TryGetValue(userId, out var dto);
        return Task.FromResult(dto);
    }
}
