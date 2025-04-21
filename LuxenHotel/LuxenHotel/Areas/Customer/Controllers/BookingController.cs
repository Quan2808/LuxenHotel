using LuxenHotel.Data;
using LuxenHotel.Models.Entities.Booking;
using LuxenHotel.Models.ViewModels.Booking;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LuxenHotel.Areas.Customer.Controllers;

[Area("Customer")]
public class BookingController : Controller
{

    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public BookingController(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    // Action hiển thị danh sách chỗ ở hoặc giao diện đặt phòng
    [HttpGet]
    public async Task<IActionResult> Accommodations()
    {
        var accommodations = await _context.Accommodations
        .Include(a => a.Combos)
        .Include(a => a.AccommodationServices)
        .ToListAsync();

        var viewModel = accommodations.Select(a => new AccommodationViewModel
        {
            Id = a.Id,
            Name = a.Name,
            Price = a.Price,
            Description = a.Description,
            Media = a.Media
        }).ToList();

        return View(viewModel);
    }

    // Action hiển thị thông tin chỗ ở
    [HttpGet]
    [Route("Accommodations/{id}")]
    public async Task<IActionResult> AccommodationDetails(int? id)
    {
        if (id == null) return NotFound();

        var accommodation = await _context.Accommodations
            .Include(a => a.Combos)
            .Include(a => a.AccommodationServices)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (accommodation == null) return NotFound();

        var viewModel = new AccommodationViewModel
        {
            Id = accommodation.Id,
            Name = accommodation.Name,
            Price = accommodation.Price,
            Description = accommodation.Description,
            Media = accommodation.Media,
            IsAvailable = accommodation.IsAvailable,
            MaxOccupancy = accommodation.MaxOccupancy,
            Area = accommodation.Area,
            CreatedAt = accommodation.CreatedAt,

            Services = accommodation.AccommodationServices?
                .Select(s => s.Service.Name)
                .ToList() ?? new List<string>(),

            Combos = accommodation.Combos?
                .Select(c => new ComboViewModel
                {
                    Name = c.Name,
                    DiscountPercent = c.DiscountPercent
                }).ToList() ?? new List<ComboViewModel>()
        };

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
    public async Task<IActionResult> Create([Bind("Name,Price,Description")] Accommodation accommodation, List<IFormFile> MediaFiles)
    {
        if (ModelState.IsValid)
        {
            var mediaPaths = new List<string>();
            if (MediaFiles != null && MediaFiles.Any())
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "media");
                Directory.CreateDirectory(uploadsFolder); // Tạo thư mục nếu chưa tồn tại

                foreach (var file in MediaFiles)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadsFolder, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        mediaPaths.Add($"/media/{fileName}");
                    }
                }
            }

            accommodation.UpdateMedia(mediaPaths);
            _context.Add(accommodation);
            await _context.SaveChangesAsync();
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

    private bool AccommodationExists(int id)
    {
        return _context.Accommodations.Any(e => e.Id == id);
    }
}