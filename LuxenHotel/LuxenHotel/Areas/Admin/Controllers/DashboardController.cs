using Microsoft.AspNetCore.Mvc;

namespace LuxenHotel.Areas.Admin.Controllers
{
    public class DashboardController : AdminBaseController
    {
        public DashboardController(ILogger<AdminBaseController> logger) : base(logger)
        {
        }

        public IActionResult Index()
        {
            // SetPageTitle("Bookings");
            return View();
        }
    }
}