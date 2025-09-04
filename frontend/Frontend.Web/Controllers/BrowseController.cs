using Microsoft.AspNetCore.Mvc;

namespace Frontend.Web.Controllers;

public class BrowseController : Controller
{
	// 分類鍵 → wwwroot/images/experts/<folder>
	// 先只開 interior，其餘留空代表尚未上架
	private static readonly Dictionary<string, string> FolderMap = new(StringComparer.OrdinalIgnoreCase)
	{
		["interior"] = "interior",
		["cleaning"] = "cleaning",
		["design"] = "design",
		["wedding"] = "wedding",
		["tutor"] = "tutor",
		["repair"] = "repair",
		["photo"] = "photo",
		["fitness"] = "fitness",
		["marketing"] = "marketing",
		["writing"] = "writing"
	};

	/// <summary>回傳 8 張圖磚的 Partial（Ajax 用）</summary>
	[HttpGet]
	public IActionResult CategoryTiles(string cat = "interior")
	{
		if (!FolderMap.TryGetValue(cat ?? "", out var folder) || string.IsNullOrEmpty(folder))
			return PartialView("~/Views/Shared/Partials/_CategoryTiles.cshtml",
				new CategoryTilesVm(cat, Array.Empty<string>()));

		// 組出 8 張圖片的虛擬路徑（~/images/...）
		var pics = Enumerable.Range(1, 8)
			.Select(i => $"~/images/experts/{folder}/{i:D2}.jpg")
			.ToArray();

		return PartialView("~/Views/Shared/Partials/_CategoryTiles.cshtml",
			new CategoryTilesVm(cat, pics));
	}

	// 簡單的 VM
	public record CategoryTilesVm(string Cat, IReadOnlyList<string> Pics);
}
