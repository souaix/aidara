using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Net.Http;

namespace Backend.Infrastructure.Localization
{
    public static class LanguageHelper
    {
        /// <summary>
        /// 自動偵測使用者語言（優先順序：?lang= → Header → 預設 zh）
        /// </summary>
        public static string DetectLanguage(HttpContext context)
        {
            // 1️⃣ Query 參數最優先 (?lang=zh-cn)
            if (context.Request.Query.TryGetValue("lang", out var qLang) && !string.IsNullOrWhiteSpace(qLang))
                return Normalize(qLang.ToString());

            // 2️⃣ 其次是 Accept-Language Header
            var headerLang = context.Request.Headers["Accept-Language"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(headerLang))
            {
                try
                {
                    // e.g. "zh-CN,zh;q=0.9,en;q=0.8"
                    var culture = CultureInfo
                        .GetCultureInfo(headerLang.Split(',')[0].Trim())
                        .TwoLetterISOLanguageName;
                    return Normalize(headerLang);
                }
                catch
                {
                    // ignore invalid format
                }
            }

            // 3️⃣ 預設繁體中文
            return "zh";
        }

        /// <summary>
        /// 標準化語言代碼（例如 zh-TW→zh, zh-CN→zh-cn）
        /// </summary>
        private static string Normalize(string lang)
        {
            lang = lang.ToLowerInvariant().Trim();
            if (lang.StartsWith("zh-cn") || lang.Contains("simplified")) return "zh-cn";
            if (lang.StartsWith("zh-tw") || lang.Contains("traditional")) return "zh";
            if (lang.StartsWith("en")) return "en";
            return "zh"; // default
        }
    }
}
