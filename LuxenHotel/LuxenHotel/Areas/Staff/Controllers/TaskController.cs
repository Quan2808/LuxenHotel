using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LuxenHotel.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "Staff")]
    public class TaskController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}