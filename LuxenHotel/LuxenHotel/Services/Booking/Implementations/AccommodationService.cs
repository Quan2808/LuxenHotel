using LuxenHotel.Data;
using LuxenHotel.Models.Entities.Booking;
using LuxenHotel.Models.ViewModels.Booking;
using LuxenHotel.Services.Booking.Interfaces;
using LuxenHotel.Utils;
using Microsoft.EntityFrameworkCore;

namespace LuxenHotel.Services.Booking.Implementations;

public class AccommodationService : IAccommodationService
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public AccommodationService(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<List<AccommodationViewModel>> GetAllAccommodationsAsync()
    {
        var accommodations = await _context.Accommodations
            .Include(a => a.Combos)
            .ToListAsync();

        return accommodations.Select(a => new AccommodationViewModel
        {
            Id = a.Id,
            Name = a.Name,
            Price = a.Price,
            Description = a.Description,
            ExistingMedia = a.Media
        }).ToList();
    }

    public async Task<AccommodationViewModel> GetAccommodationByIdAsync(int? id)
    {
        if (id == null)
            return null;

        var accommodation = await _context.Accommodations
            .Include(a => a.Combos)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (accommodation == null)
            return null;

        return new AccommodationViewModel
        {
            Id = accommodation.Id,
            Name = accommodation.Name,
            Price = accommodation.Price,
            Description = accommodation.Description,
            ExistingMedia = accommodation.Media,
            IsAvailable = accommodation.IsAvailable,
            MaxOccupancy = accommodation.MaxOccupancy,
            Area = accommodation.Area,
            CreatedAt = accommodation.CreatedAt,
            // Combos = accommodation.Combos?
            //     .Select(c => new ComboViewModel { Name = c.Name })
            //     .ToList() ?? new List<ComboViewModel>()
        };
    }

    public async Task CreateAccommodationAsync(Accommodation accommodation, List<IFormFile> mediaFiles)
    {
        if (accommodation == null)
            throw new ArgumentNullException(nameof(accommodation));

        var mediaPaths = await FileUploadUtility.UploadFilesAsync(mediaFiles, _environment);
        accommodation.UpdateMedia(mediaPaths);

        if (accommodation.CreatedAt == default)
            accommodation.CreatedAt = DateTime.UtcNow;

        foreach (var service in accommodation.Services)
        {
            service.Accommodation = accommodation;
            service.CreatedAt = DateTime.UtcNow;
        }

        _context.Accommodations.Add(accommodation);
        await _context.SaveChangesAsync();
    }
}