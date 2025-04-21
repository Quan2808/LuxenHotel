
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxenHotel.Models.Entities.Booking;

public class AccommodationService
{
    [Key]
    [Column(Order = 1)]
    [ForeignKey("Accommodation")]
    public int AccommodationId { get; set; }

    [Key]
    [Column(Order = 2)]
    [ForeignKey("Service")]
    public int ServiceId { get; set; }

    [Column(TypeName = "decimal(8,2)")]
    public decimal? AdditionalPrice { get; set; }

    public required Accommodation Accommodation { get; set; }
    public required Service Service { get; set; }
}