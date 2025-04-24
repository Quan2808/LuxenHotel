using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LuxenHotel.Data;
using LuxenHotel.Models.Entities.Booking;
using LuxenHotel.Areas.Admin.Controllers;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace LuxenHotel.Areas.Admin.Controllers
{
    // [Route("Accommodation")]
    public class AccommodationManagementController : AdminBaseController
    {
        private readonly ApplicationDbContext _dbContext;

        public AccommodationManagementController(ILogger<AdminBaseController> logger, ApplicationDbContext dbContext)
            : base(logger)
        {
            _dbContext = dbContext;
        }

        // GET: /Admin/AccommodationManagement
        public async Task<IActionResult> Index()
        {
            SetPageTitle("Accommodations");
            LogInfo("Accessed Accommodations list");

            var accommodations = await _dbContext.Accommodations
                .Include(a => a.Services)
                .ToListAsync();
            return View(accommodations);
        }

        // GET: /Admin/AccommodationManagement/Create
        public IActionResult Create()
        {
            SetPageTitle("Add New Accommodation");
            return View();
        }

        // POST: /Admin/AccommodationManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Accommodation model)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Accommodations.Add(model);
                await _dbContext.SaveChangesAsync();
                AddNotification("Accommodation created successfully.", NotificationType.Success);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // GET: /Admin/AccommodationManagement/Edit/1
        public async Task<IActionResult> Edit(int id)
        {
            SetPageTitle("Edit Accommodation");
            var accommodation = await _dbContext.Accommodations
                .Include(a => a.Services)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (accommodation == null)
            {
                return HandleError("Accommodation not found.");
            }
            return View(accommodation);
        }

        // POST: /Admin/AccommodationManagement/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Accommodation model)
        {
            if (id != model.Id)
            {
                return HandleError("Invalid accommodation ID.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _dbContext.Update(model);
                    await _dbContext.SaveChangesAsync();
                    AddNotification("Accommodation updated successfully.", NotificationType.Success);
                }
                catch (DbUpdateException ex)
                {
                    return HandleError("Failed to update accommodation.", ex);
                }
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // POST: /Admin/AccommodationManagement/Delete/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var accommodation = await _dbContext.Accommodations.FindAsync(id);
            if (accommodation == null)
            {
                return HandleError("Accommodation not found.");
            }

            _dbContext.Accommodations.Remove(accommodation);
            await _dbContext.SaveChangesAsync();
            AddNotification("Accommodation deleted successfully.", NotificationType.Success);
            return RedirectToAction("Index");
        }

        // GET: /Admin/AccommodationManagement/Services/1
        public async Task<IActionResult> Services(int accommodationId)
        {
            SetPageTitle("Services");
            LogInfo($"Accessed Services for Accommodation ID: {accommodationId}");

            var accommodation = await _dbContext.Accommodations
                .Include(a => a.Services)
                .FirstOrDefaultAsync(a => a.Id == accommodationId);

            if (accommodation == null)
            {
                return HandleError("Accommodation not found.");
            }
            return View(accommodation);
        }

        // GET: /Admin/AccommodationManagement/CreateService/1
        public IActionResult CreateService(int accommodationId)
        {
            SetPageTitle("Add New Service");
            ViewBag.AccommodationId = accommodationId;
            return View();
        }

        // POST: /Admin/AccommodationManagement/CreateService/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateService(int accommodationId, Service model)
        {
            if (ModelState.IsValid)
            {
                var accommodation = await _dbContext.Accommodations.FindAsync(accommodationId);
                if (accommodation == null)
                {
                    return HandleError("Accommodation not found.");
                }

                model.AccommodationId = accommodationId;
                _dbContext.Services.Add(model);
                await _dbContext.SaveChangesAsync();
                AddNotification("Service created successfully.", NotificationType.Success);
                return RedirectToAction("Services", new { accommodationId });
            }
            ViewBag.AccommodationId = accommodationId;
            return View(model);
        }

        // GET: /Admin/AccommodationManagement/EditService/1
        public async Task<IActionResult> EditService(int id)
        {
            SetPageTitle("Edit Service");
            var service = await _dbContext.Services
                .Include(s => s.Accommodation)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (service == null)
            {
                return HandleError("Service not found.");
            }
            ViewBag.AccommodationId = service.AccommodationId;
            return View(service);
        }

        // POST: /Admin/AccommodationManagement/EditService/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditService(int id, Service model)
        {
            if (id != model.Id)
            {
                return HandleError("Invalid service ID.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _dbContext.Update(model);
                    await _dbContext.SaveChangesAsync();
                    AddNotification("Service updated successfully.", NotificationType.Success);
                }
                catch (DbUpdateException ex)
                {
                    return HandleError("Failed to update service.", ex);
                }
                return RedirectToAction("Services", new { accommodationId = model.AccommodationId });
            }
            ViewBag.AccommodationId = model.AccommodationId;
            return View(model);
        }

        // POST: /Admin/AccommodationManagement/DeleteService/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteService(int id)
        {
            var service = await _dbContext.Services.FindAsync(id);
            if (service == null)
            {
                return HandleError("Service not found.");
            }

            var accommodationId = service.AccommodationId;
            _dbContext.Services.Remove(service);
            await _dbContext.SaveChangesAsync();
            AddNotification("Service deleted successfully.", NotificationType.Success);
            return RedirectToAction("Services", new { accommodationId });
        }
    }
}