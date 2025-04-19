using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxenHotel.Models.Entities.Booking;

public class Accommodation : BaseEntity
{
    [Key]
    public int AccommodationId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [Required]
    [Column(TypeName = "decimal(8,2)")]
    public decimal Price { get; set; }

    public string? Description { get; set; }

    public List<Combo> Combos { get; set; } = new List<Combo>();
    public List<AccommodationService> AccommodationServices { get; set; } = new List<AccommodationService>();
}