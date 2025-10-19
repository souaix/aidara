// Infrastructure/Persistence/Mock/MockServiceStatRepo.cs
using System.Data;
using Backend.Application.Ports;
using Backend.Application.ViewModels.Services;

namespace Backend.Infrastructure.Persistence.Mock;

public sealed class MockServiceStatRepo : IBossServiceStatRepo
{
    public Task<List<ServiceStatVm>> GetServiceStatsAsync(IDbConnection conn, IDbTransaction? tx, Guid itemId, CancellationToken ct)
    {
        var fake = new List<ServiceStatVm>
        {
            new() { CityId = "台北市", DistrictId = "文山區", StoreCount = 12 },
            new() { CityId = "台北市", DistrictId = "大安區", StoreCount = 34 },
            new() { CityId = "台北市", DistrictId = "信義區", StoreCount = 56 },
            new() { CityId = "新北市", DistrictId = "新店區", StoreCount = 62 },
            new() { CityId = "新北市", DistrictId = "永和區", StoreCount = 37 },
            new() { CityId = "新北市", DistrictId = "新莊區", StoreCount = 16 },
            new() { CityId = "新北市", DistrictId = "瑞芳區", StoreCount = 42 },
            new() { CityId = "新北市", DistrictId = "石碇區", StoreCount = 28 },
            new() { CityId = "新北市", DistrictId = "三峽區", StoreCount = 11 },
            new() { CityId = "新北市", DistrictId = "鶯歌區", StoreCount = 12 },
        };

        return Task.FromResult(fake);
    }
}
