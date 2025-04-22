using System.ComponentModel.DataAnnotations;

namespace LuxenHotel.Models.ViewModels.Booking;

public class ComboViewModel
{
    [Required(ErrorMessage = "Combo name is required")]
    [StringLength(255, ErrorMessage = "Combo name cannot exceed 255 characters")]
    public string Name { get; set; }

    [Range(0, 100, ErrorMessage = "Discount percent must be between 0 and 100")]
    public decimal DiscountPercent { get; set; }
}