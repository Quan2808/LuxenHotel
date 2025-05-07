using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxenHotel.Models.Entities.Booking;

public class ComboService
{
    public int ComboId { get; set; }
    public Combo? Combo { get; set; }

    public int ServiceId { get; set; }
    public Service? Service { get; set; }
}