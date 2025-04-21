
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxenHotel.Models.Entities.Booking;

public class Booking
{
    public int Id { get; set; }
    public int AccommodationId { get; set; }
    public Accommodation Accommodation { get; set; }
    public int? ComboId { get; set; }
    public Combo Combo { get; set; }
    [Required]
    public string CustomerName { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}