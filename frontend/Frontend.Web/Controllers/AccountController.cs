using Frontend.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
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
        // 🧩 先印出看看後端實際回傳的 JSON
        Console.WriteLine("==== Backend JSON Response ====");
        Console.WriteLine(json);
        Console.WriteLine("===============================");
        var result = JsonSerializer.Deserialize<EnsureUserResponse>(json, _jsonOpts);
        

        if (result?.User is null) return LocalRedirect("/");


    
        // 3) 簽站內 Cookie（用後端回來的 UserDto）
        var user = result.User;  // ✅ 補上這一行
                                 // 新增角色清單（從後端回傳）
        var roles = user.Roles;

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.DisplayName ?? user.Email)
        };
        if (!string.IsNullOrEmpty(user.AvatarUrl))
            claims.Add(new Claim("avatar_url", user.AvatarUrl));
        // 💡 新增角色資訊
        foreach (var role in roles)
        {
            Console.WriteLine($"User has role: {role}");
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(id),
            new AuthenticationProperties { IsPersistent = true });


        if (result.IsNew)
        {
            TempData["FlashMessage"] = "🎉 歡迎加入 SamaSama！你獲得了 100 銀幣！";
            TempData["FlashType"] = "success";
        }

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

	public IActionResult Beaboss()
	{
		return View("BeABoss"); // 對應 Views/Account/BeABoss.cshtml
	}

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> RefreshClaims(string returnUrl = "/")
    {
        Console.WriteLine("🔥 RefreshClaims triggered by " + User.Identity?.Name);

        // 1) 取目前登入者 UserId
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
            return LocalRedirect("/");

        // 2) 呼叫 Backend.Api 拿最新的 Profile 與 Role
        var client = _httpClientFactory.CreateClient("BackendApi");

        var profileResp = await client.GetAsync($"/api/BossInfo/user/{userId}");
        if (!profileResp.IsSuccessStatusCode) return LocalRedirect("/");

        var roleResp = await client.GetAsync($"/api/BossInfo/role/{userId}");
        if (!roleResp.IsSuccessStatusCode) return LocalRedirect("/");

        var profileJson = await profileResp.Content.ReadAsStringAsync();
        var roleJson = await roleResp.Content.ReadAsStringAsync();

        // 你已有對應 DTO；這裡簡化成 dynamic 讀值
        using var doc1 = JsonDocument.Parse(profileJson);
        using var doc2 = JsonDocument.Parse(roleJson);

        var email = doc1.RootElement.GetProperty("email").GetString() ?? "";
        var displayName = doc1.RootElement.GetProperty("displayName").GetString() ?? email;
        var avatarUrl = doc1.RootElement.TryGetProperty("avatarUrl", out var av) ? av.GetString() ?? "" : "";
        var roleId = doc2.RootElement.GetProperty("roleId").GetString() ?? "UNVERIFYBOSS";

        // 3) 重新簽 Cookie（帶入最新 Role）
        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        new Claim(ClaimTypes.Email, email),
        new Claim(ClaimTypes.Name, displayName),
    };
        if (!string.IsNullOrWhiteSpace(avatarUrl)) claims.Add(new Claim("avatar_url", avatarUrl));

        // 重要：加入角色
        claims.Add(new Claim(ClaimTypes.Role, roleId));

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(id),
            new AuthenticationProperties { IsPersistent = true });

        Console.WriteLine($"ProfileResp: {profileResp.StatusCode}");
        Console.WriteLine($"RoleResp: {roleResp.StatusCode}");
        var body1 = await profileResp.Content.ReadAsStringAsync();
        var body2 = await roleResp.Content.ReadAsStringAsync();
        Console.WriteLine("PROFILE_JSON => " + body1);
        Console.WriteLine("ROLE_JSON => " + body2);


        // 4) 導回
        return LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
    }


    [Authorize]
    [HttpPost]
    public async Task<IActionResult> SwitchMode([FromBody] string roleId, string returnUrl = "/")
    {
        Console.WriteLine($"🔁 SwitchMode requested: {roleId}");

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId)) return LocalRedirect("/");

        var client = _httpClientFactory.CreateClient("BackendApi");

        // 1️⃣ 更新後端 ActiveMode
        using var content = new StringContent(JsonSerializer.Serialize(new { RoleId = roleId }), Encoding.UTF8, "application/json");
        var resp = await client.PostAsync($"/api/UserRole/switch-active-mode/{userId}", content);

        if (!resp.IsSuccessStatusCode)
        {
            Console.WriteLine($"⚠️ SwitchMode failed: {resp.StatusCode}");
            return LocalRedirect(returnUrl);
        }

        // 2️⃣ 重新抓最新角色與模式
        var rolesResp = await client.GetAsync($"/api/UserRole/{userId}");
        var modeResp = await client.GetAsync($"/api/UserRole/active-mode/{userId}");

        var rolesJson = await rolesResp.Content.ReadAsStringAsync();
        var modeJson = await modeResp.Content.ReadAsStringAsync();

        using var doc1 = JsonDocument.Parse(rolesJson);
        using var doc2 = JsonDocument.Parse(modeJson);

        // rolesJson 是一個陣列
        var roles = doc1.RootElement.EnumerateArray().Select(x => x.GetProperty("RoleId").GetString() ?? "").ToList();
        var activeMode = doc2.RootElement.GetProperty("roleId").GetString() ?? "CUSTOMER";

        // 3️⃣ 重簽 cookie（帶入最新角色與 active_mode）
        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        new Claim(ClaimTypes.Email, User.FindFirstValue(ClaimTypes.Email) ?? ""),
        new Claim(ClaimTypes.Name, User.Identity?.Name ?? ""),
        new Claim("active_mode", activeMode)
    };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
            new AuthenticationProperties { IsPersistent = true });

        Console.WriteLine($"✅ SwitchMode success: ActiveMode={activeMode}, Roles={string.Join(',', roles)}");

        return LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
    }

}
