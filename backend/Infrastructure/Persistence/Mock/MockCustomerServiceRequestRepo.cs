// Infrastructure/Persistence/Mock/MockCustomerServiceRequestRepo.cs
using System.Collections.Concurrent;
using System.Data;
using Backend.Application.Ports;
using Backend.Application.ViewModels.Services;

namespace Backend.Infrastructure.Persistence.Mock;

public sealed class MockCustomerServiceRequestRepo : ICustomerServiceRequestRepo
{
    private readonly ConcurrentBag<CustomerServiceRequestDto> _requests = new();

    public Task InsertAsync(IDbConnection conn, IDbTransaction? tx, CustomerServiceRequestDto dto, CancellationToken ct)
    {
        _requests.Add(dto);
        Console.WriteLine($"[Mock] 收到一筆 CustomerServiceRequest: " +
                          $"UserId={dto.UserId}, ItemId={dto.ItemId}, " +
                          $"PriceMin={dto.PriceMin}, PriceMax={dto.PriceMax}, " +
                          $"Methods=[{string.Join(",", dto.Methods)}]");
        return Task.CompletedTask;
    }

    // 測試時可用來檢查結果
    public IEnumerable<CustomerServiceRequestDto> GetAll() => _requests;
}
