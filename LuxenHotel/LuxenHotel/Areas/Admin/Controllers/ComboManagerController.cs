using LuxenHotel.Services.Booking.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LuxenHotel.Areas.Admin.Controllers
{
    [Route("admin/combos")]
    public class ComboManagerController : AdminBaseController
    {
        private readonly IComboService _comboService;

        public ComboManagerController(
            ILogger<AdminBaseController> logger,
            IComboService comboService) : base(logger)
        {
            _comboService = comboService ?? throw new ArgumentNullException(nameof(comboService));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var combos = await _comboService.ListAsync();
            SetPageTitle("Combos Management");
            LogInfo("Viewed list of combos");
            return View(combos);
        }
    }
}