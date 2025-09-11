using Microsoft.AspNetCore.Mvc;

namespace Frontend.Web.Controllers
{
    public class BossServicesController : Controller
    {
        public IActionResult Beaboss()
        {
            return View(); // 會去找 /Views/BossServices/beaboss.cshtml
        }
    }
}
