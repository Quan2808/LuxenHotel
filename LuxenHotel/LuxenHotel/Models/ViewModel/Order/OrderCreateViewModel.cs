using LuxenHotel.Models.Entities.Orders;

namespace LuxenHotel.Models.ViewModels;

public class OrderCreateViewModel
{
    public int AccommodationId { get; set; }
    public string CustomerName { get; set; }
    public string CustomerEmail { get; set; }
    public string CustomerPhone { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfGuests { get; set; }
    public string SpecialRequests { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public List<int> SelectedServiceIds { get; set; } = new List<int>();
    public List<int> ServiceQuantities { get; set; } = new List<int>();
    public List<int> SelectedComboIds { get; set; } = new List<int>();
    public List<int> ComboQuantities { get; set; } = new List<int>();
}