using Microsoft.AspNetCore.Mvc;

namespace Frontend.Web.Controllers;

public class MapController : Controller
{
    public IActionResult Index()
    {
        // 回傳 Views/Map/Index.cshtml
        return View();
    }
}
