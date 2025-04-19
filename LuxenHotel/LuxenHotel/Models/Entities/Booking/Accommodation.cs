using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace LuxenHotel.Models.Entities.Booking;

public class Accommodation : BaseEntity
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true
    };

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [Required]
    [Column(TypeName = "decimal(8,2)")]
    [Range(0, double.MaxValue, ErrorMessage = "Price must be non-negative")]
    public decimal Price { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string Description { get; set; }

    [NotMapped]
    public List<string> Media
    {
        get => string.IsNullOrEmpty(MediaJson)
            ? new List<string>()
            : JsonSerializer.Deserialize<List<string>>(MediaJson, _jsonOptions) ?? new List<string>();
        set => MediaJson = value == null || value.Count == 0
            ? null
            : JsonSerializer.Serialize(value, _jsonOptions);
    }

    [Column("Media", TypeName = "nvarchar(max)")]
    public string? MediaJson { get; private set; }

    public List<Combo> Combos { get; set; } = new List<Combo>();
    public List<AccommodationService> AccommodationServices { get; set; } = new List<AccommodationService>();

    // Helper method to safely update Media
    public void UpdateMedia(List<string> media)
    {
        Media = media ?? new List<string>();
    }
}