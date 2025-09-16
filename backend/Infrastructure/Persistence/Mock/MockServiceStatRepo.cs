//地區服務項目統計列表

using Backend.Application.ViewModels.Services;

public class MockServiceStatRepo : IServiceStatRepo
{
	public Task<List<ServiceStatVm>> GetServiceStatsAsync(Guid itemId, CancellationToken ct)
	{
		var fake = new List<ServiceStatVm>
		{
			new ServiceStatVm { CityId = "台北市", DistrictId = "文山區", StoreCount = 12 },
			new ServiceStatVm { CityId = "台北市", DistrictId = "大安區", StoreCount = 34 },
			new ServiceStatVm { CityId = "台北市", DistrictId = "信義區", StoreCount = 56 },
			new ServiceStatVm { CityId = "新北市", DistrictId = "新店區", StoreCount = 62 },
			new ServiceStatVm { CityId = "新北市", DistrictId = "永和區", StoreCount = 37 },
			new ServiceStatVm { CityId = "新北市", DistrictId = "新莊區", StoreCount = 16 },
			new ServiceStatVm { CityId = "新北市", DistrictId = "瑞芳區", StoreCount = 42 },
			new ServiceStatVm { CityId = "新北市", DistrictId = "石碇區", StoreCount = 28 },
			new ServiceStatVm { CityId = "新北市", DistrictId = "三峽區", StoreCount = 11 },
			new ServiceStatVm { CityId = "新北市", DistrictId = "鶯歌區", StoreCount = 12 },

		};
		return Task.FromResult(fake);
	}
}
