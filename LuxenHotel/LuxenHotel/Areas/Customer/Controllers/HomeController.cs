using Microsoft.AspNetCore.Mvc;

namespace LuxenHotel.Areas.Customer.Controllers
{
    [Area("Customer")]
    // [Authorize(Roles = "Customer")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View("~/Areas/Customer/Views/Home/Index/Index.cshtml");
        }

        public IActionResult About()
        {
            return View("~/Areas/Customer/Views/Home/About/Index.cshtml");
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}