using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LuxenHotel.Models.ViewModels.Booking;

public class AccommodationCreateViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(255, ErrorMessage = "Name cannot exceed 255 characters")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Price is required")]
    [Range(0, 50000000, ErrorMessage = "Price must be between 0 and 50,000,000")]
    public int Price { get; set; }

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Max occupancy is required")]
    [Range(1, 50, ErrorMessage = "Max occupancy must be between 1 and 50")]
    public int MaxOccupancy { get; set; }

    [Required(ErrorMessage = "Area is required")]
    [Range(0, 10000, ErrorMessage = "Area must be between 0 and 10,000 m²")]
    public decimal Area { get; set; }

    public bool IsAvailable { get; set; } = true;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ServiceViewModel> Services { get; set; } = new List<ServiceViewModel>();
}