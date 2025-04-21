using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace LuxenHotel.Models.Entities.Booking;

public class Accommodation
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true
    };

    [Key]
    public int Id { get; set; }
    [Required]
    public required string Name { get; set; }

    [Required]
    [Column(TypeName = "decimal(8,2)")]
    [Range(0, double.MaxValue, ErrorMessage = "Price must be non-negative")]
    public decimal Price { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? Description { get; set; }

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

    public bool IsAvailable { get; set; } = true; // Có sẵn mặc định 

    [Required]
    public int MaxOccupancy { get; set; } // Số khách tối đa 

    [Column(TypeName = "decimal(10,0)")]
    [Required]
    public decimal Area { get; set; } // Diện tích (m²) 

    public DateTime? CreatedAt { get; set; } = DateTime.Now;

    public List<AccommodationService> AccommodationServices { get; set; }
    public List<Combo> Combos { get; set; }
    public List<Booking> Bookings { get; set; }


    // Helper method to safely update Media
    public void UpdateMedia(List<string> media)
    {
        Media = media ?? new List<string>();
    }
}