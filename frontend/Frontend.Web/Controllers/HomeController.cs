// Frontend.Web.Controllers.HomeController.cs
using Frontend.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

public class HomeController : Controller
{
	private readonly IHttpClientFactory _http;

	public HomeController(IHttpClientFactory httpClientFactory)
	{
		_http = httpClientFactory;
	}

	public async Task<IActionResult> Index()
	{
		if (User.Identity?.IsAuthenticated ?? false)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (Guid.TryParse(userId, out var uid))
			{
				var client = _http.CreateClient("BackendApi");
				var url = $"/api/wallet/balances?userId={uid}&currencies=Gold,Silver";
				var resp = await client.GetAsync(url);

				if (resp.IsSuccessStatusCode)
				{
					var json = await resp.Content.ReadAsStringAsync();
					var balances = JsonSerializer.Deserialize<List<BalanceViewModel>>(json, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					ViewBag.GoldBalance = balances?.FirstOrDefault(b => b.Currency == "Gold")?.Balance ?? 0;
					ViewBag.SilverBalance = balances?.FirstOrDefault(b => b.Currency == "Silver")?.Balance ?? 0;
				}
			}
		}

		return View();
	}
}
