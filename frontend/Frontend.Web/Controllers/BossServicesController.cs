using Frontend.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Frontend.Web.Controllers
{
    public class BossServicesController : Controller
    {
        public IActionResult Beaboss()
        {
            return View(); // 會去找 /Views/BossServices/beaboss.cshtml
        }

        /// <summary>
        /// 編輯共用服務設定
        /// </summary>
        [HttpGet("/BossServices/EditStore")]
        public IActionResult EditStore()
        {
            // 從登入資訊取得 UserId
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            // 建立 ViewModel
            var vm = new BossStoreEditVm
            {
                UserId = userId
            };

            // 回傳部分視圖（partial）
            return PartialView("___BossStoreFormPartial", vm);
        }

        /// <summary>
        /// 編輯既有服務項目
        /// </summary>
        [HttpGet("/BossServices/Edit/{itemId}")]
        public IActionResult Edit(string itemId)
        {
            // 從登入資訊取得 UserId
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            // 建立 ViewModel
            var vm = new BossServiceEditVm
            {
                UserId = userId,
                ItemId = itemId
            };

            // 回傳部分視圖（partial）
            return PartialView("___BossServiceFormPartial", vm);
        }

        /// <summary>
        /// 新增服務項目（空白表單）
        /// </summary>
        [HttpGet("/BossServices/Create")]
        public IActionResult Create()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var vm = new BossServiceEditVm
            {
                UserId = userId,
                ItemId = null
            };

            return PartialView("___BossServiceFormPartial", vm);
        }
    }
}
