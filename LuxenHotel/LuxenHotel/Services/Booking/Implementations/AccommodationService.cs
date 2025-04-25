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

    public async Task<List<AccommodationViewModel>> ListAsync()
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

    public async Task<AccommodationViewModel> GetAsync(int? id)
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

    public async Task CreateAsync(AccommodationViewModel viewModel)
    {
        if (viewModel == null)
            throw new ArgumentNullException(nameof(viewModel));

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

        if (viewModel.MediaFiles != null && viewModel.MediaFiles.Any())
        {
            var mediaPaths = await FileUploadUtility.UploadFilesAsync(viewModel.MediaFiles, _environment);
            accommodation.UpdateMedia(mediaPaths);
        }

        if (viewModel.Services != null && viewModel.Services.Any())
        {
            foreach (var serviceViewModel in viewModel.Services)
            {
                if (!string.IsNullOrEmpty(serviceViewModel.Name))
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
    }
}