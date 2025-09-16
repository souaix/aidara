//問卷服務項目

using Backend.Application.Ports;
using Backend.Application.ViewModels.Services;

namespace Infrastructure.Persistence.Mock;

/// <summary>
/// 假的 ServiceRepo，不連資料庫，直接回傳固定分類/子分類/項目
/// </summary>
public class MockServiceRepo : IServiceRepo
{
	public Task<List<ServiceCategoryVm>> GetAllCategoriesAsync(CancellationToken ct)
	{
		// 模擬三個大分類，每個大分類三個子分類，每個子分類三個項目
		var categories = new List<ServiceCategoryVm>();

		for (int c = 1; c <= 3; c++)
		{
			var cat = new ServiceCategoryVm
			{
				CategoryId = Guid.NewGuid(),
				Name = $"假大分類 {c}"
			};

			for (int s = 1; s <= 3; s++)
			{
				var sub = new ServiceSubcategoryVm
				{
					SubcategoryId = Guid.NewGuid(),
					Name = $"假子分類 {c}-{s}"
				};

				for (int i = 1; i <= 3; i++)
				{
					sub.Items.Add(new ServiceItemVm
					{
						ItemId = Guid.NewGuid(),
						Name = $"假服務項目 {c}-{s}-{i}"
					});
				}

				cat.Subcategories.Add(sub);
			}

			categories.Add(cat);
		}

		return Task.FromResult(categories);
	}
}
