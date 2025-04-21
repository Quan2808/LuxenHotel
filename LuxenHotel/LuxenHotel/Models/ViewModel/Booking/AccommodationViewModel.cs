namespace LuxenHotel.Models.ViewModels.Booking;

public class AccommodationViewModel
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required decimal Price { get; set; }
    public string? Description { get; set; }
    public List<string> Media { get; set; } = new();
}
