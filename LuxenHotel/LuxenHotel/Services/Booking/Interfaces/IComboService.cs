using LuxenHotel.Models.Entities.Booking;
using LuxenHotel.Models.ViewModels.Booking;

namespace LuxenHotel.Services.Booking.Interfaces
{
    public interface IComboService
    {
        Task<List<ComboViewModel>> ListAsync();
        Task<List<ComboViewModel>> GetCombosByAccommodationIdAsync(int accommodationId);
        Task<Combo?> GetComboByIdAsync(int comboId);
        Task<Combo> CreateComboAsync(Combo combo, List<int> selectedServiceIds);
        Task<Combo?> UpdateComboAsync(int comboId, Combo combo);
        Task<bool> DeleteComboAsync(int comboId);
    }
}