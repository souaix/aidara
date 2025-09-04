using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Frontend.Web.Controllers;

public class AccountController : Controller
{
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
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!result.Succeeded)
        {
            // 有些情況 Cookie 尚未建立，用 HttpContext.AuthenticateAsync() 讀不到，
            // 實務上可直接從 HttpContext.User 取得 External 登入資料：
        }

        // 從外部身分取使用者資訊
        var principal = HttpContext.User;
        var email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var name = principal.Identity?.Name ?? email ?? "User";

        if (string.IsNullOrEmpty(email))
            return Redirect("/"); // 取不到 email，作保守處理

        // TODO: 查 DB 是否已有該 email
        var exists = false; // 假設查詢結果

        if (mode.Equals("register", StringComparison.OrdinalIgnoreCase))
        {
            if (!exists)
            {
                // TODO: 建立新帳號（寫入 DB）
                exists = true;
            }
            // 若已存在也當登入處理
        }
        else // login
        {
            if (!exists)
            {
                // 依你的需求：可以導到註冊提示頁，或自動建立
                // 這裡示範直接導回 Modal，顯示訊息（可自訂）
                return Redirect("/");
            }
        }

        // 3) 建立本地登入 Cookie（用你系統的 UserId/Role…）
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Email, email),
            // new Claim(ClaimTypes.NameIdentifier, userId), // 之後接 DB 可加
            // new Claim(ClaimTypes.Role, "User"),
        };

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
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return LocalRedirect(returnUrl);
    }

    // 若你還保留舊的「帳密登入」頁，可用這個 action 導頁
    public IActionResult Login() => Redirect("/");
}
