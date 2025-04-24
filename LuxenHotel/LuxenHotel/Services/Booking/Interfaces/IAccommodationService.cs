using LuxenHotel.Models.Entities.Booking;
using LuxenHotel.Models.ViewModels.Booking;

namespace LuxenHotel.Services.Booking.Interfaces;

public interface IAccommodationService
{
    Task<List<AccommodationViewModel>> GetAllAccommodationsAsync();
    Task<AccommodationViewModel> GetAccommodationByIdAsync(int? id);
    Task CreateAccommodationAsync(Accommodation accommodation, List<IFormFile> mediaFiles);
}