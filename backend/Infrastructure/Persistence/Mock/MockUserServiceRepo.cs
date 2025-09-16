
using Backend.Application.Ports;
using Backend.Application.ViewModels.Services;

namespace Infrastructure.Persistence.Mock;

/// <summary>
/// 假的 UserServiceRepo，不連資料庫，直接回傳固定資料
/// </summary>
public class MockUserServiceRepo : IUserServiceRepo
{
	public Task<List<UserServiceItemVm>> GetUserServicesAsync(Guid userId, CancellationToken ct)
	{
		// 模擬該 userId 已選擇的服務項目
		var fake = new List<UserServiceItemVm>
		{
			new()
			{
				ItemId = Guid.NewGuid(),
				ItemName = "居家清潔",
				SubcategoryId = Guid.NewGuid(),
				SubcategoryName = "清掃",
				CategoryId = Guid.NewGuid(),
				CategoryName = "家務服務"
			},
			new()
			{
				ItemId = Guid.NewGuid(),
				ItemName = "水電修繕",
				SubcategoryId = Guid.NewGuid(),
				SubcategoryName = "維修",
				CategoryId = Guid.NewGuid(),
				CategoryName = "居家修繕"
			}
		};

		return Task.FromResult(fake);
	}

	public Task UpdateUserServicesAsync(Guid userId, List<Guid> itemIds, CancellationToken ct)
	{
		// 什麼都不做，直接完成
		return Task.CompletedTask;
	}

}
