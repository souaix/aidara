using Microsoft.AspNetCore.Mvc;

namespace Frontend.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
