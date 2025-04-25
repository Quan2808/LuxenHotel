using LuxenHotel.Models.Entities.Booking;
using LuxenHotel.Models.ViewModels.Booking;

namespace LuxenHotel.Services.Booking.Interfaces;

public interface IAccommodationService
{
    Task<List<AccommodationViewModel>> ListAsync();
    Task<AccommodationViewModel> GetAsync(int? id);
    Task CreateAsync(AccommodationViewModel viewModel);
}