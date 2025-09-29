using Microsoft.AspNetCore.Mvc;

namespace Frontend.Web.Controllers
{
    public class StripeController : Controller
    {
        // GET: /Stripe/Test
        public IActionResult Test()
        {
            return View();
        }
    }
}
