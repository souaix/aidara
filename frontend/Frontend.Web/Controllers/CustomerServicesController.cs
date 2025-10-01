using Microsoft.AspNetCore.Mvc;

namespace Frontend.Web.Controllers
{
    public class CustomerServicesController : Controller
    {
        public IActionResult RequestService()
        {
            return View(); // Views/CustomerServices/RequestService.cshtml
        }

        public IActionResult BossMode()
        {
            return View();
        }
    }

}
