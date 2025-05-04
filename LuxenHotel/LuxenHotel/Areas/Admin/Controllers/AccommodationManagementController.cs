using LuxenHotel.Models.Entities.Booking;
using LuxenHotel.Models.ViewModels.Booking;
using LuxenHotel.Services.Booking.Interfaces;
using LuxenHotel.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace LuxenHotel.Areas.Admin.Controllers
{
    [Route("admin/accommodations")]
    public class AccommodationManagementController : AdminBaseController
    {
        private readonly IAccommodationService _accommodationService;

        public AccommodationManagementController(
            ILogger<AdminBaseController> logger,
            IAccommodationService accommodationService)
            : base(logger)
        {
            _accommodationService = accommodationService ?? throw new ArgumentNullException(nameof(accommodationService));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var accommodations = await _accommodationService.ListAsync();
            SetPageTitle("Accommodations Management");
            LogInfo("Viewed list of accommodations");
            return View(accommodations);
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            var viewModel = new AccommodationViewModel
            {
                Services = [new ServiceViewModel()]
            };
            SetPageTitle("Add New Accommodation");
            LogInfo("Accessed add accommodation page");
            return View("Save", viewModel);
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AccommodationViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                AddNotification("Invalid input data", NotificationType.Error);
                LogInfo("Invalid input data for creating accommodation");

                foreach (var entry in ModelState)
                {
                    var fieldName = entry.Key;
                    var state = entry.Value;

                    if (state != null && state.Errors.Count > 0)
                    {
                        foreach (var error in state.Errors)
                        {
                            LogInfo($"Validation error in field '{fieldName}': {error.ErrorMessage}");
                        }
                    }
                }

                return View("Save", viewModel);
            }

            await _accommodationService.CreateAsync(viewModel);
            AddNotification("Accommodation created successfully", NotificationType.Success);
            LogInfo($"Created accommodation: {viewModel.Name}");
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!id.HasValue)
                return RedirectWithError("Invalid accommodation ID");

            var viewModel = await _accommodationService.GetAsync(id.Value);
            if (viewModel == null)
                return RedirectWithError("Accommodation not found");

            if (!viewModel.Services.Any())
                viewModel.Services.Add(new ServiceViewModel());

            SetPageTitle($"Edit {viewModel.Name}");
            LogInfo($"Accessed edit page for accommodation ID: {id}");
            return View("Save", viewModel);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AccommodationViewModel viewModel)
        {
            if (id != viewModel.Id)
                return RedirectWithError("Invalid accommodation ID");

            if (!ModelState.IsValid)
            {
                AddNotification("Invalid input data", NotificationType.Error);
                LogInfo($"Invalid input data for editing accommodation ID: {id}");
                return View(viewModel);
            }

            // Process MediaToDelete if it's a JSON string
            if (Request.Form["MediaToDelete"].Count > 0)
            {
                string mediaToDeleteJson = Request.Form["MediaToDelete"];
                if (!string.IsNullOrEmpty(mediaToDeleteJson))
                {
                    try
                    {
                        List<string> mediaToDelete = JsonSerializer.Deserialize<List<string>>(mediaToDeleteJson);
                        viewModel.MediaToDelete = mediaToDelete;
                    }
                    catch (JsonException ex)
                    {
                        LogInfo($"Error parsing MediaToDelete JSON: {ex.Message}");
                    }
                }
            }

            try
            {
                await _accommodationService.EditAsync(id, viewModel);
                AddNotification("Accommodation updated successfully", NotificationType.Success);
                LogInfo($"Updated accommodation ID: {id}");
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                AddNotification(ex.Message, NotificationType.Error);
                LogInfo($"Failed to update accommodation ID: {id}, Error: {ex.Message}");
                return View(viewModel);
            }
        }

        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!id.HasValue)
                return RedirectWithError("Invalid accommodation ID");

            var viewModel = await _accommodationService.GetAsync(id.Value);
            if (viewModel == null)
                return RedirectWithError("Accommodation not found");

            SetPageTitle($"Delete {viewModel.Name}");
            LogInfo($"Accessed delete page for accommodation ID: {id}");
            return View(viewModel);
        }

        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var viewModel = await _accommodationService.GetAsync(id);
                if (viewModel == null)
                    return RedirectWithError("Accommodation not found");

                await _accommodationService.DeleteAsync(id);
                AddNotification("Accommodation deleted successfully", NotificationType.Success);
                LogInfo($"Deleted accommodation ID: {id}");
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                AddNotification(ex.Message, NotificationType.Error);
                LogInfo($"Failed to delete accommodation ID: {id}, Error: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        private IActionResult RedirectWithError(string message)
        {
            AddNotification(message, NotificationType.Error);
            return RedirectToAction(nameof(Index));
        }
    }
}