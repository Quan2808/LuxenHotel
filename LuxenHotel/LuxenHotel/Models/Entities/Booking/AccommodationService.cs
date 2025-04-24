
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxenHotel.Models.Entities.Booking;

public class AccommodationService
{
    [Required]
    public int AccommodationId { get; set; }

    public Accommodation? Accommodation { get; set; }

    [Required]
    public int ServiceId { get; set; }

    public Service? Service { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}