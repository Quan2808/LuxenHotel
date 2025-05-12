using LuxenHotel.Models.Entities.Booking;
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
            var combos = await _comboService.ListAsync();
            var accommodations = await _accommodationService.GetDropdownListAsync();

            var accommodationServices = new Dictionary<int, List<ServiceViewModel>>();
            foreach (var accommodation in accommodations)
            {
                var services = await _accommodationService.GetServicesForAccommodationAsync(accommodation.Id);
                accommodationServices.Add(accommodation.Id, services);
            }

            var viewModel = new ComboListViewModel
            {
                Combos = combos,
                Accommodations = accommodations,
                AccommodationServices = accommodationServices
            };

            SetPageTitle("Combos Management");
            LogInfo("Viewed list of combos");
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ComboViewModel model)
        {
            if (!ModelState.IsValid)
            {
                HandleInvalidModelState();
                return RedirectToAction(nameof(Index), model);
            }

            try
            {
                var accommodation = await _accommodationService.GetAsync(model.AccommodationId);
                if (accommodation == null)
                {
                    TempData["ErrorMessage"] = "The selected accommodation does not exist.";
                    return RedirectToAction(nameof(Index));
                }

                var combo = new Combo
                {
                    Name = model.Name,
                    Price = model.Price,
                    Description = model.Description,
                    AccommodationId = model.AccommodationId,
                    Status = model.Status
                };

                await _comboService.CreateComboAsync(combo, model.SelectedServiceIds);

                TempData["SuccessMessage"] = "Combo created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating combo: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}