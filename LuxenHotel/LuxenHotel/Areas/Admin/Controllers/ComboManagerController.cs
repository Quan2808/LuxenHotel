using LuxenHotel.Models.ViewModels.Booking;
using LuxenHotel.Services.Booking.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LuxenHotel.Areas.Admin.Controllers
{
    [Route("admin/combos")]
    public class ComboManagerController : AdminBaseController
    {
        private readonly IComboService _comboService;
        private readonly IAccommodationService _accommodationService;

        public ComboManagerController(
            ILogger<AdminBaseController> logger,
            IComboService comboService,
            IAccommodationService accommodationService) : base(logger)
        {
            _comboService = comboService
                ?? throw new ArgumentNullException(nameof(comboService));
            _accommodationService = accommodationService
                ?? throw new ArgumentNullException(nameof(accommodationService));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = new ComboListViewModel
            {
                Combos = await _comboService.ListAsync(),
                Accommodations = await _accommodationService.GetDropdownListAsync()
            };

            SetPageTitle("Combos Management");
            LogInfo("Viewed list of combos");
            return View(viewModel);
        }
    }
}