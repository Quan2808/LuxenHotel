using LuxenHotel.Data;
using LuxenHotel.Models.Entities.Booking;
using LuxenHotel.Models.ViewModels.Booking;
using LuxenHotel.Services.Booking.Interfaces;
using LuxenHotel.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LuxenHotel.Services.Booking.Implementations
{
    public class AccommodationService : IAccommodationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AccommodationService(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public async Task<List<AccommodationViewModel>> ListAsync()
        {
            var accommodations = await _context.Accommodations
                .AsNoTracking()
                .Include(a => a.Services)
                .ToListAsync();

            return accommodations.Select(ToViewModel).ToList();
        }

        public async Task<AccommodationViewModel?> GetAsync(int? id)
        {
            if (!id.HasValue)
                return null;

            var accommodation = await _context.Accommodations
                .AsNoTracking()
                .Include(a => a.Services)
                .FirstOrDefaultAsync(m => m.Id == id.Value);

            return accommodation == null ? null : ToViewModel(accommodation);
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

            await UpdateMediaAsync(accommodation, viewModel);
            UpdateServices(accommodation, viewModel.Services);

            _context.Accommodations.Add(accommodation);
            await _context.SaveChangesAsync();
        }

        public async Task EditAsync(int id, AccommodationViewModel viewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            var accommodation = await _context.Accommodations
                .Include(a => a.Services)
                .FirstOrDefaultAsync(a => a.Id == id)
                ?? throw new InvalidOperationException($"Accommodation with ID {id} not found.");

            // Update basic properties
            accommodation.Name = viewModel.Name;
            accommodation.Price = viewModel.Price;
            accommodation.Description = viewModel.Description;
            accommodation.MaxOccupancy = viewModel.MaxOccupancy;
            accommodation.Area = viewModel.Area;
            accommodation.IsAvailable = viewModel.IsAvailable;
            accommodation.UpdatedAt = DateTime.UtcNow;

            await UpdateMediaAsync(accommodation, viewModel);
            UpdateServices(accommodation, viewModel.Services);

            await _context.SaveChangesAsync();
        }

        private async Task UpdateMediaAsync(Accommodation accommodation, AccommodationViewModel viewModel)
        {
            // Handle media uploads
            if (viewModel.MediaFiles?.Any() == true)
            {
                var mediaPaths = await FileUploadUtility.UploadFilesAsync(viewModel.MediaFiles, _environment);
                accommodation.UpdateMedia(mediaPaths);
            }

            // Handle thumbnail upload
            if (viewModel.ThumbnailFile?.Length > 0)
            {
                var thumbnailPath = await FileUploadUtility.UploadSingleFileAsync(viewModel.ThumbnailFile, _environment);
                if (thumbnailPath != null)
                {
                    if (!string.IsNullOrEmpty(accommodation.Thumbnail))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, accommodation.Thumbnail.TrimStart('/'));
                        if (File.Exists(oldFilePath))
                            File.Delete(oldFilePath);
                    }
                    accommodation.Thumbnail = thumbnailPath;
                }
            }
        }

        private void UpdateServices(Accommodation accommodation, IList<ServiceViewModel>? services)
        {
            accommodation.Services.Clear();
            if (services?.Any() != true)
                return;

            foreach (var serviceViewModel in services)
            {
                if (!string.IsNullOrEmpty(serviceViewModel.Name))
                {
                    accommodation.Services.Add(new Service
                    {
                        Name = serviceViewModel.Name,
                        Price = serviceViewModel.Price,
                        Description = serviceViewModel.Description,
                        CreatedAt = DateTime.UtcNow,
                        Accommodation = accommodation
                    });
                }
            }
        }

        private AccommodationViewModel ToViewModel(Accommodation accommodation) => new()
        {
            Id = accommodation.Id,
            Name = accommodation.Name,
            Price = accommodation.Price,
            Description = accommodation.Description,
            Media = accommodation.Media,
            Thumbnail = accommodation.Thumbnail,
            Services = accommodation.Services.Select(s => new ServiceViewModel
            {
                Id = s.Id,
                Name = s.Name,
                Price = s.Price,
                Description = s.Description
            }).ToList(),
            IsAvailable = accommodation.IsAvailable,
            MaxOccupancy = accommodation.MaxOccupancy,
            Area = accommodation.Area,
            CreatedAt = accommodation.CreatedAt
        };

        public async Task DeleteAsync(int id)
        {
            var accommodation = await _context.Accommodations
                .Include(a => a.Services)
                .FirstOrDefaultAsync(a => a.Id == id)
                ?? throw new InvalidOperationException($"Accommodation with ID {id} not found.");

            // Delete associated media files
            if (!string.IsNullOrEmpty(accommodation.Thumbnail))
            {
                var thumbnailPath = Path.Combine(_environment.WebRootPath, accommodation.Thumbnail.TrimStart('/'));
                if (File.Exists(thumbnailPath))
                    File.Delete(thumbnailPath);
            }

            if (accommodation.Media?.Any() == true)
            {
                foreach (var mediaPath in accommodation.Media)
                {
                    var fullPath = Path.Combine(_environment.WebRootPath, mediaPath.TrimStart('/'));
                    if (File.Exists(fullPath))
                        File.Delete(fullPath);
                }
            }

            _context.Accommodations.Remove(accommodation);
            await _context.SaveChangesAsync();
        }
    }
}