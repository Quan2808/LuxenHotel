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
    public async Task<IActionResult> Create(
        [Bind("Name,Price,Description")] Accommodation accommodation,
        List<IFormFile> MediaFiles)
    {
        if (ModelState.IsValid)
        {
            await _accommodationService.CreateAccommodationAsync(accommodation, MediaFiles);
            return RedirectToAction(nameof(Accommodations));
        }
        return View(accommodation);
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