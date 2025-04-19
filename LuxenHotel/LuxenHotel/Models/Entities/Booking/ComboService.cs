using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxenHotel.Models.Entities.Booking;

public class ComboService
{
    [Key]
    [Column(Order = 1)]
    [ForeignKey("Combo")]
    public int ComboId { get; set; }

    [Key]
    [Column(Order = 2)]
    [ForeignKey("Service")]
    public int ServiceId { get; set; }

    public Combo Combo { get; set; }
    public Service Service { get; set; }
}
