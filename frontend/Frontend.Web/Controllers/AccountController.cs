// Frontend.Web/Controllers/AccountController.cs
using Frontend.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Frontend.Web.Controllers
{
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

        // 1️⃣ Google 登入入口
        [HttpGet]
        public IActionResult Google(string mode = "login", string returnUrl = "/")
        {
            var redirectUrl = Url.Action(nameof(GoogleCallback), "Account", new { mode, returnUrl });
            var props = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(props, GoogleDefaults.AuthenticationScheme);
        }

        // 2️⃣ Google 回調
        [HttpGet]
        public async Task<IActionResult> GoogleCallback(string mode = "login", string returnUrl = "/")
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var email = User.FindFirstValue(ClaimTypes.Email);
            var name = User.Identity?.Name ?? email ?? "User";
            var avatar = User.Claims.FirstOrDefault(c => c.Type == "urn:google:picture")?.Value
                      ?? User.Claims.FirstOrDefault(c => c.Type == "picture")?.Value;

            if (string.IsNullOrWhiteSpace(email))
                return LocalRedirect("/");

            // 2️⃣ 呼叫後端確保或建立使用者
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
                return Content($"Backend error {resp.StatusCode}\n{body}", "text/plain", Encoding.UTF8);
            }

            var json = await resp.Content.ReadAsStringAsync();
            Console.WriteLine("==== Backend JSON Response ====");
            Console.WriteLine(json);
            Console.WriteLine("===============================");

            var result = JsonSerializer.Deserialize<EnsureUserResponse>(json, _jsonOpts);
            if (result?.User is null) return LocalRedirect("/");

            var user = result.User;
            var roles = user.Roles;

            // 3️⃣ 建立 Cookie Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.DisplayName ?? user.Email),
                new Claim("active_mode", roles.FirstOrDefault() ?? "CUSTOMER")
            };

            if (!string.IsNullOrEmpty(user.AvatarUrl))
                claims.Add(new Claim("avatar_url", user.AvatarUrl));

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
                Console.WriteLine($"User has role: {role}");
            }

            var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(id),
                new AuthenticationProperties { IsPersistent = true }
            );

            if (result.IsNew)
            {
                TempData["FlashMessage"] = "🎉 歡迎加入 SamaSama！你獲得了 100 銀幣！";
                TempData["FlashType"] = "success";
            }

            return LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
        }

        // 3️⃣ 登出
        [Authorize]
        public async Task<IActionResult> Logout(string returnUrl = "/")
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
        }

        public IActionResult Login() => Redirect("/?auth=1");

        public IActionResult Beaboss()
        {
            return View("BeABoss");
        }

        // 4️⃣ 切換使用模式 (B方案：redirect 同頁)
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SwitchMode([FromBody] string roleId, string returnUrl = "/")
        {
            Console.WriteLine($"🔁 SwitchMode requested: {roleId}");

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return LocalRedirect("/");

            var client = _httpClientFactory.CreateClient("BackendApi");

            // 1️⃣ 更新後端 ActiveMode
            using var content = new StringContent(JsonSerializer.Serialize(new { RoleId = roleId }), Encoding.UTF8, "application/json");
            var resp = await client.PostAsync($"/api/UserRole/user/{userId}/switch-active-mode", content);

            if (!resp.IsSuccessStatusCode)
            {
                Console.WriteLine($"⚠️ SwitchMode failed: {resp.StatusCode}");
                return Ok(new { success = false });
            }

            // 2️⃣ 拿最新角色 + 模式
            var rolesResp = await client.GetAsync($"/api/UserRole/user/{userId}/available-roles");
            var modeResp = await client.GetAsync($"/api/UserRole/user/{userId}/get-active-mode");

            var rolesJson = await rolesResp.Content.ReadAsStringAsync();
            var modeJson = await modeResp.Content.ReadAsStringAsync();

            using var doc1 = JsonDocument.Parse(rolesJson);
            using var doc2 = JsonDocument.Parse(modeJson);

            var roles = doc1.RootElement.EnumerateArray()
                .Select(x => x.GetProperty("RoleId").GetString() ?? "")
                .ToList();

            var activeMode = doc2.RootElement.GetProperty("roleId").GetString() ?? "CUSTOMER";

            // 3️⃣ 重簽 cookie
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

            // B方案：回傳 JSON，前端 redirect 到同頁
            return Ok(new { success = true, activeMode = activeMode });
        }
    }
}
