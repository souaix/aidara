using Frontend.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Frontend.Web.Controllers;

public class AccountController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public AccountController(IHttpClientFactory httpClientFactory)
    => _httpClientFactory = httpClientFactory;

    // 1) 入口：導去 Google
    [HttpGet]
    public IActionResult Google(string mode = "login", string returnUrl = "/")
    {
        // 把 mode/returnUrl 夾帶到回調
        var redirectUrl = Url.Action(nameof(GoogleCallback), "Account", new { mode, returnUrl });
        var props = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(props, GoogleDefaults.AuthenticationScheme);
    }

    // 2) Google 回調
    [HttpGet]
    public async Task<IActionResult> GoogleCallback(string mode = "login", string returnUrl = "/")
    {
		await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

		Console.WriteLine("[Claims]");
		foreach (var c in User.Claims)
		{
			Console.WriteLine($"  {c.Type} = {c.Value}");
		}


		// 1) 從 Google Claims 取資料
		var email = User.FindFirstValue(ClaimTypes.Email);
        var name = User.Identity?.Name ?? email ?? "User";
        var avatar = User.Claims.FirstOrDefault(c => c.Type == "urn:google:picture")?.Value
                  ?? User.Claims.FirstOrDefault(c => c.Type == "picture")?.Value;

        if (string.IsNullOrWhiteSpace(email))
            return LocalRedirect("/");

        // 2) 呼叫 Backend.Api 確認/建立使用者
        var client = _httpClientFactory.CreateClient("BackendApi");
        var payload = new GoogleEnsurePayload(
            Email: email,
            DisplayName: name,
            AvatarUrl: avatar,
            Mode: mode.Equals("register", StringComparison.OrdinalIgnoreCase) ? AuthMode.Register : AuthMode.Login
        );

        using var content = new StringContent(JsonSerializer.Serialize(payload, _jsonOpts), Encoding.UTF8, "application/json");
        var resp = await client.PostAsync("/api/auth/google/auto", content);

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync();
            // 先暫時用 200 顯示錯誤內容
            return Content($"Backend 400/500：{resp.StatusCode}\n{body}", "text/plain", Encoding.UTF8);
        }


        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // login 模式但後端查無帳號（且不自動註冊）
            return LocalRedirect("/");
        }

        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync();
        var user = JsonSerializer.Deserialize<UserDto>(json, _jsonOpts);
        if (user is null) return LocalRedirect("/");

        // 3) 簽站內 Cookie（用後端回來的 UserDto）
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.DisplayName ?? user.Email)
        };
        if (!string.IsNullOrEmpty(user.AvatarUrl))
            claims.Add(new Claim("avatar_url", user.AvatarUrl));

        var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(id),
            new AuthenticationProperties { IsPersistent = true });

        return LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
    }

    [Authorize]
    public async Task<IActionResult> Logout(string returnUrl = "/")
    {
        // 登出本站（Cookie）
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
    }

    // 若你還保留舊的「帳密登入」頁，可用這個 action 導頁
    public IActionResult Login() => Redirect("/?auth=1");

}
