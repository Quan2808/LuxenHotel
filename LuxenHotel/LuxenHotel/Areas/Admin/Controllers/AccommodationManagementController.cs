using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LuxenHotel.Data;
using LuxenHotel.Models.Entities.Booking;
using LuxenHotel.Areas.Admin.Controllers;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using LuxenHotel.Services.Booking.Interfaces;
using LuxenHotel.Models.ViewModels.Booking;

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
            _accommodationService = accommodationService;
        }

        // GET: /admin/accommodations
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                SetPageTitle("Accommodations Management");
                var accommodations = await _accommodationService.ListAsync();
                LogInfo("Viewed list of accommodations");
                return View(accommodations);
            }
            catch (Exception ex)
            {
                return HandleError("Failed to load accommodations", ex);
            }
        }

        // GET: admin/accommodations/details/1
        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null)
                {
                    AddNotification("Invalid accommodation ID", NotificationType.Error);
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = await _accommodationService.GetAsync(id);
                if (viewModel == null)
                {
                    AddNotification("Accommodation not found", NotificationType.Error);
                    return RedirectToAction(nameof(Index));
                }

                SetPageTitle($"Details of {viewModel.Name}");
                LogInfo($"Viewed details of accommodation ID: {id}");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                return HandleError($"Failed to load details for accommodation ID: {id}", ex);
            }
        }

        // GET: admin/accommodations/create
        [HttpGet("create")]
        public IActionResult Create()
        {
            try
            {
                SetPageTitle("Create New Accommodation");
                var viewModel = new AccommodationViewModel
                {
                    Services = new List<ServiceViewModel> { new ServiceViewModel() }
                };
                LogInfo("Accessed create accommodation page");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                return HandleError("Failed to load create accommodation page", ex);
            }
        }

        // POST: admin/accommodations/create
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AccommodationViewModel viewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    AddNotification("Invalid input data", NotificationType.Error);
                    LogInfo("Invalid input data for creating accommodation");
                    return View(viewModel);
                }

                await _accommodationService.CreateAsync(viewModel);
                AddNotification("Accommodation created successfully", NotificationType.Success);
                LogInfo($"Created accommodation: {viewModel.Name}");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return HandleError("Failed to create accommodation", ex);
            }
        }

        // GET: /admin/accommodations/{id}/edit
        // [HttpGet("{id}/edit")]
        // public async Task<IActionResult> Edit(int id)
        // {
        //     SetPageTitle("Edit Accommodation");
        //     var accommodation = await _dbContext.Accommodations
        //         .Include(a => a.Services)
        //         .FirstOrDefaultAsync(a => a.Id == id);

        //     if (accommodation == null)
        //     {
        //         return HandleError("Accommodation not found.");
        //     }
        //     return View(accommodation);
        // }

        // PUT: /admin/accommodations/{id}
        // [HttpPut("{id}")]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Edit(int id, Accommodation model)
        // {
        //     if (id != model.Id)
        //     {
        //         return HandleError("Invalid accommodation ID.");
        //     }

        //     if (ModelState.IsValid)
        //     {
        //         try
        //         {
        //             _dbContext.Update(model);
        //             await _dbContext.SaveChangesAsync();
        //             AddNotification("Accommodation updated successfully.", NotificationType.Success);
        //         }
        //         catch (DbUpdateException ex)
        //         {
        //             return HandleError("Failed to update accommodation.", ex);
        //         }
        //         return RedirectToAction("Index");
        //     }
        //     return View(model);
        // }

        // // DELETE: /admin/accommodations/{id}
        // [HttpDelete("{id}")]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Delete(int id)
        // {
        //     var accommodation = await _dbContext.Accommodations.FindAsync(id);
        //     if (accommodation == null)
        //     {
        //         return HandleError("Accommodation not found.");
        //     }

        //     _dbContext.Accommodations.Remove(accommodation);
        //     await _dbContext.SaveChangesAsync();
        //     AddNotification("Accommodation deleted successfully.", NotificationType.Success);
        //     return RedirectToAction("Index");
        // }

        // // GET: /admin/accommodations/{accommodationId}/services
        // [HttpGet("{accommodationId}/services")]
        // public async Task<IActionResult> Services(int accommodationId)
        // {
        //     SetPageTitle("Services");
        //     LogInfo($"Accessed Services for Accommodation ID: {accommodationId}");

        //     var accommodation = await _dbContext.Accommodations
        //         .Include(a => a.Services)
        //         .FirstOrDefaultAsync(a => a.Id == accommodationId);

        //     if (accommodation == null)
        //     {
        //         return HandleError("Accommodation not found.");
        //     }
        //     return View(accommodation);
        // }

        // // GET: /admin/accommodations/{accommodationId}/services/create
        // [HttpGet("{accommodationId}/services/create")]
        // public IActionResult CreateService(int accommodationId)
        // {
        //     SetPageTitle("Add New Service");
        //     ViewBag.AccommodationId = accommodationId;
        //     return View();
        // }

        // // POST: /admin/accommodations/{accommodationId}/services
        // [HttpPost("{accommodationId}/services")]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> CreateService(int accommodationId, Service model)
        // {
        //     if (ModelState.IsValid)
        //     {
        //         var accommodation = await _dbContext.Accommodations.FindAsync(accommodationId);
        //         if (accommodation == null)
        //         {
        //             return HandleError("Accommodation not found.");
        //         }

        //         model.AccommodationId = accommodationId;
        //         _dbContext.Services.Add(model);
        //         await _dbContext.SaveChangesAsync();
        //         AddNotification("Service created successfully.", NotificationType.Success);
        //         return RedirectToAction("Services", new { accommodationId });
        //     }
        //     ViewBag.AccommodationId = accommodationId;
        //     return View(model);
        // }

        // // GET: /admin/accommodations/services/{id}/edit
        // [HttpGet("services/{id}/edit")]
        // public async Task<IActionResult> EditService(int id)
        // {
        //     SetPageTitle("Edit Service");
        //     var service = await _dbContext.Services
        //         .Include(s => s.Accommodation)
        //         .FirstOrDefaultAsync(s => s.Id == id);

        //     if (service == null)
        //     {
        //         return HandleError("Service not found.");
        //     }
        //     ViewBag.AccommodationId = service.AccommodationId;
        //     return View(service);
        // }

        // // PUT: /admin/accommodations/services/{id}
        // [HttpPut("services/{id}")]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> EditService(int id, Service model)
        // {
        //     if (id != model.Id)
        //     {
        //         return HandleError("Invalid service ID.");
        //     }

        //     if (ModelState.IsValid)
        //     {
        //         try
        //         {
        //             _dbContext.Update(model);
        //             await _dbContext.SaveChangesAsync();
        //             AddNotification("Service updated successfully.", NotificationType.Success);
        //         }
        //         catch (DbUpdateException ex)
        //         {
        //             return HandleError("Failed to update service.", ex);
        //         }
        //         return RedirectToAction("Services", new { accommodationId = model.AccommodationId });
        //     }
        //     ViewBag.AccommodationId = model.AccommodationId;
        //     return View(model);
        // }

        // // DELETE: /admin/accommodations/services/{id}
        // [HttpDelete("services/{id}")]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> DeleteService(int id)
        // {
        //     var service = await _dbContext.Services.FindAsync(id);
        //     if (service == null)
        //     {
        //         return HandleError("Service not found.");
        //     }

        //     var accommodationId = service.AccommodationId;
        //     _dbContext.Services.Remove(service);
        //     await _dbContext.SaveChangesAsync();
        //     AddNotification("Service deleted successfully.", NotificationType.Success);
        //     return RedirectToAction("Services", new { accommodationId });
        // }
    }
}