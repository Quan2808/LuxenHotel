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

        [HttpGet("")]
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

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                // Get the combo with its services
                var combo = await _comboService.GetComboByIdAsync(id);
                if (combo == null)
                {
                    TempData["ErrorMessage"] = "Combo not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Get all accommodations for dropdown
                var accommodations = await _accommodationService.GetDropdownListAsync();

                // Get services for the combo's accommodation
                var accommodationServices = await _accommodationService.GetServicesForAccommodationAsync(combo.AccommodationId);

                // Create the view model
                var viewModel = new ComboViewModel
                {
                    Id = combo.Id,
                    Name = combo.Name,
                    Price = combo.Price,
                    Description = combo.Description,
                    AccommodationId = combo.AccommodationId,
                    AccommodationName = combo.Accommodation?.Name,
                    Status = combo.Status,
                    // Get the IDs of the services associated with this combo
                    SelectedServiceIds = combo.ComboServices.Select(s => s.Id).ToList(),
                    Services = accommodationServices,
                    CreatedAt = combo.CreatedAt
                };

                // Store accommodation services in ViewBag for the view to use
                ViewBag.Accommodations = accommodations;
                ViewBag.AccommodationServices = new Dictionary<int, IEnumerable<ServiceViewModel>>
                {
                    { combo.AccommodationId, accommodationServices }
                };
                SetPageTitle($"Edit {viewModel.Name}");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading combo: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [Route("edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ComboViewModel model)
        {
            if (!ModelState.IsValid)
            {
                HandleInvalidModelState();

                // Re-populate the data for the view
                await PrepareEditViewData(model);
                return View(model);
            }

            try
            {
                // Validate the accommodation
                var accommodation = await _accommodationService.GetAsync(model.AccommodationId);
                if (accommodation == null)
                {
                    TempData["ErrorMessage"] = "The selected accommodation does not exist.";
                    return RedirectToAction(nameof(Index));
                }

                // Get the existing combo to ensure it exists
                var existingCombo = await _comboService.GetComboByIdAsync(model.Id);
                if (existingCombo == null)
                {
                    TempData["ErrorMessage"] = "Combo not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Create the combo entity from the view model
                var combo = new Combo
                {
                    Id = model.Id,
                    Name = model.Name,
                    Price = model.Price,
                    Description = model.Description,
                    AccommodationId = model.AccommodationId,
                    Status = model.Status,
                    // Keep the original creation date
                    CreatedAt = existingCombo.CreatedAt
                };

                // Update the combo and its service relationships
                await _comboService.UpdateComboAsync(combo, model.SelectedServiceIds);

                TempData["SuccessMessage"] = "Combo updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating combo: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // Helper method to prepare the view data
        private async Task PrepareEditViewData(ComboViewModel model)
        {
            var accommodations = await _accommodationService.GetDropdownListAsync();
            var accommodationServices = await _accommodationService.GetServicesForAccommodationAsync(model.AccommodationId);

            ViewBag.Accommodations = accommodations;
            ViewBag.AccommodationServices = new Dictionary<int, IEnumerable<ServiceViewModel>>
            {
                { model.AccommodationId, accommodationServices }
            };
        }
    }
}