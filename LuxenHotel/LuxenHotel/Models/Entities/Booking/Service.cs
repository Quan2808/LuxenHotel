
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxenHotel.Models.Entities.Booking;

public class Service
{
    [Key]
    public int Id { get; set; }

    [Required]
    public required string Name { get; set; }

    [Required]
    [Column(TypeName = "decimal(8,2)")]
    public decimal Price { get; set; }

    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<AccommodationService> AccommodationServices { get; set; }
    public List<ComboService> ComboServices { get; set; }
}