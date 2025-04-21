
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxenHotel.Models.Entities.Booking;

public class AccommodationService
{
    public int AccommodationId { get; set; }
    public Accommodation Accommodation { get; set; }
    public int ServiceId { get; set; }
    public Service Service { get; set; }
}