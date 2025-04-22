using System.ComponentModel.DataAnnotations;

namespace LuxenHotel.Models.ViewModels.Booking;

public class ServiceViewModel
{
    [Required(ErrorMessage = "Service name is required")]
    [StringLength(255, ErrorMessage = "Service name cannot exceed 255 characters")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Service price is required")]
    [Range(0, 50000000, ErrorMessage = "Service price must be between 0 and 50,000,000")]
    public int Price { get; set; }

    [StringLength(1000, ErrorMessage = "Service description cannot exceed 1000 characters")]
    public string? Description { get; set; }
}