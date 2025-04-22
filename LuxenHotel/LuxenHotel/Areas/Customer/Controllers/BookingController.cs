using LuxenHotel.Data;
using LuxenHotel.Models.Entities.Booking;
using LuxenHotel.Models.ViewModels.Booking;
using LuxenHotel.Services.Booking.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LuxenHotel.Areas.Customer.Controllers;

[Area("Customer")]
public class BookingController : Controller
{

    private readonly IAccommodationService _accommodationService;

    public BookingController(IAccommodationService accommodationService)
    {
        _accommodationService = accommodationService;
    }

    [HttpGet]
    public async Task<IActionResult> Accommodations()
    {
        var viewModel = await _accommodationService.GetAllAccommodationsAsync();
        return View(viewModel);
    }

    [HttpGet]
    [Route("Accommodations/{id}")]
    public async Task<IActionResult> AccommodationDetails(int? id)
    {
        var viewModel = await _accommodationService.GetAccommodationByIdAsync(id);
        if (viewModel == null)
            return NotFound();

        return View(viewModel);
    }

    // GET: Accommodation/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Accommodation/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AccommodationCreateViewModel viewModel, List<IFormFile> MediaFiles)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        try
        {
            var accommodation = new Accommodation
            {
                Name = viewModel.Name,
                Price = viewModel.Price,
                Description = viewModel.Description,
                MaxOccupancy = viewModel.MaxOccupancy,
                Area = viewModel.Area,
                IsAvailable = viewModel.IsAvailable,
                Services = viewModel.Services?.Select(s => new Service
                {
                    Name = s.Name,
                    Price = s.Price,
                    Description = s.Description,
                    CreatedAt = DateTime.UtcNow
                }).ToList() ?? new List<Service>()
            };

            await _accommodationService.CreateAccommodationAsync(accommodation, MediaFiles);
            TempData["SuccessMessage"] = "Accommodation and services created successfully!";
            return RedirectToAction(nameof(Accommodations));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"An error occurred: {ex.Message}");
            return View(viewModel);
        }
    }

    // Action xử lý đặt chỗ ở
    [HttpPost]
    public ActionResult BookAccommodations()
    {
        return View();
    }

    // Action hiển thị danh sách dịch vụ hoặc giao diện đặt dịch vụ
    [HttpGet]
    public ActionResult Services()
    {
        return View();
    }

    // Action xử lý đặt dịch vụ
    [HttpPost]
    public ActionResult BookServices()
    {
        return View();
    }
}