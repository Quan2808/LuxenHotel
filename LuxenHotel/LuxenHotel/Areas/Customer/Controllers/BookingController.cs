using LuxenHotel.Data;
using LuxenHotel.Models.Entities.Booking;
using LuxenHotel.Models.ViewModels.Booking;
using LuxenHotel.Services.Booking.Interfaces;
using LuxenHotel.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LuxenHotel.Areas.Customer.Controllers;

[Area("Customer")]
public class BookingController : Controller
{

    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly IAccommodationService _accommodationService;

    public BookingController(IAccommodationService accommodationService,
        ApplicationDbContext context,
        IWebHostEnvironment environment)
    {
        _accommodationService = accommodationService;
        _context = context;
        _environment = environment;
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
        var viewModel = new AccommodationViewModel
        {
            Services = new List<ServiceViewModel> { new ServiceViewModel() } // Initialize with one empty service
        };
        return View(viewModel);
    }

    // POST: Accommodation/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AccommodationViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var accommodation = new Accommodation
        {
            Name = viewModel.Name,
            Price = viewModel.Price,
            Description = viewModel.Description,
            MaxOccupancy = viewModel.MaxOccupancy,
            Area = viewModel.Area,
            IsAvailable = viewModel.IsAvailable,
            CreatedAt = DateTime.UtcNow
        };

        // Handle file uploads
        if (viewModel.MediaFiles != null && viewModel.MediaFiles.Any())
        {
            var mediaPaths = await FileUploadUtility.UploadFilesAsync(viewModel.MediaFiles, _environment);
            accommodation.UpdateMedia(mediaPaths);
        }

        // Handle services
        if (viewModel.Services != null && viewModel.Services.Any())
        {
            foreach (var serviceViewModel in viewModel.Services)
            {
                if (!string.IsNullOrEmpty(serviceViewModel.Name)) // Only add valid services
                {
                    var service = new Service
                    {
                        Name = serviceViewModel.Name,
                        Price = serviceViewModel.Price,
                        Description = serviceViewModel.Description,
                        CreatedAt = DateTime.UtcNow,
                        Accommodation = accommodation
                    };
                    accommodation.Services.Add(service);
                }
            }
        }

        _context.Accommodations.Add(accommodation);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Accommodations));
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