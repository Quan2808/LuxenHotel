namespace LuxenHotel.Models.ViewModels.Booking;

public class AccommodationViewModel
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required decimal Price { get; set; }
    public string? Description { get; set; }
    public List<string> Media { get; set; } = new();

    public bool IsAvailable { get; set; }
    public int MaxOccupancy { get; set; }
    public decimal Area { get; set; }
    public DateTime? CreatedAt { get; set; }

    public List<string> Services { get; set; } = new();

    public List<ComboViewModel> Combos { get; set; } = new();
}

public class ComboViewModel
{
    public string Name { get; set; }
    public decimal DiscountPercent { get; set; }
}
