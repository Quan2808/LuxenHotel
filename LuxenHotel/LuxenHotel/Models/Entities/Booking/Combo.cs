using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxenHotel.Models.Entities.Booking;

public class Combo : BaseEntity
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("Accommodation")]
    public int? AccommodationId { get; set; }

    [Required]
    [StringLength(100)]
    public required string Name { get; set; }

    [Required]
    [Column(TypeName = "decimal(8,2)")]
    public decimal Price { get; set; }

    public string? Description { get; set; }

    public Accommodation? Accommodation { get; set; }
    public List<ComboService> ComboServices { get; set; } = new List<ComboService>();
}