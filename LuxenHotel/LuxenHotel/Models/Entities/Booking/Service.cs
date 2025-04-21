
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxenHotel.Models.Entities.Booking;

public class Service : BaseEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public required string Name { get; set; }

    [Required]
    [Column(TypeName = "decimal(8,2)")]
    public decimal Price { get; set; }

    public string? Description { get; set; }

    public List<ComboService> ComboServices { get; set; } = new List<ComboService>();
    public List<AccommodationService> AccommodationServices { get; set; } = new List<AccommodationService>();
}