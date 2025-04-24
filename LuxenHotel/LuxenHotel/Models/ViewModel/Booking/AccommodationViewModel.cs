using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LuxenHotel.Models.ViewModels.Booking;

public class AccommodationViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(255, ErrorMessage = "Name cannot exceed 255 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Price is required")]
    [Range(0, 50000000, ErrorMessage = "Price must be between 0 and 50,000,000")]
    public int Price { get; set; }

    public string? Description { get; set; }

    [Required(ErrorMessage = "Maximum occupancy is required")]
    [Range(1, 50, ErrorMessage = "Max occupancy must be between 1 and 50")]
    public int MaxOccupancy { get; set; }

    [Required(ErrorMessage = "Area is required")]
    [Range(0, 10000, ErrorMessage = "Area must be between 0 and 10,000 m²")]
    public decimal Area { get; set; }

    public bool IsAvailable { get; set; } = true;

    // For file uploads
    public List<IFormFile>? MediaFiles { get; set; }

    // To display existing media
    public List<string> ExistingMedia { get; set; } = new List<string>();

    // List of services to create
    public List<ServiceViewModel> Services { get; set; } = new List<ServiceViewModel>();

    public DateTime? CreatedAt { get; set; }
}