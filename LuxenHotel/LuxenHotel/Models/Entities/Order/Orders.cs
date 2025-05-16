using LuxenHotel.Models.Entities.Booking;
using LuxenHotel.Models.Entities.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxenHotel.Models.Entities.Orders;

public class Orders
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string OrderCode { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();

    // User information - linked to AspNetUsers
    public string? UserId { get; set; }
    public User? User { get; set; }

    // Customer information (for guest bookings)
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }

    [Required]
    public int AccommodationId { get; set; }
    public Accommodation Accommodation { get; set; }

    // Transaction ID from payment gateways
    public string? TransactionId { get; set; }

    [Required]
    public PaymentMethod PaymentMethod { get; set; }

    [Required]
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    [Required]
    public OrderStatus Status { get; set; } = OrderStatus.Created;

    [Required]
    [Column(TypeName = "int")]
    [Range(0, int.MaxValue, ErrorMessage = "Total price must be a positive number")]
    public int TotalPrice { get; set; }

    // Check-in/out dates
    [Required]
    public DateTime CheckInDate { get; set; }

    [Required]
    public DateTime CheckOutDate { get; set; }

    // Number of guests
    [Required]
    [Range(1, 50, ErrorMessage = "Number of guests must be between 1 and 50")]
    public int NumberOfGuests { get; set; }

    // Order can include either Service or Combo, not both
    public int? ServiceId { get; set; }
    public Service? Service { get; set; }
    public int? ServiceQuantity { get; set; }

    public int? ComboId { get; set; }
    public Combo? Combo { get; set; }
    public int? ComboQuantity { get; set; }

    // Special requests
    public string? SpecialRequests { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Payment and cancellation timestamps
    public DateTime? PaidAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    // Cancellation reason
    public string? CancellationReason { get; set; }

    // Calculate total price
    public void CalculateTotalPrice()
    {
        int total = Accommodation?.Price ?? 0;

        // Calculate number of stay days
        int numberOfDays = (int)Math.Ceiling((CheckOutDate - CheckInDate).TotalDays);
        if (numberOfDays < 1) numberOfDays = 1;
        total *= numberOfDays;

        // Add service price if applicable
        if (Service != null && ServiceQuantity.HasValue)
        {
            total += Service.Price * ServiceQuantity.Value;
        }

        // Add combo price if applicable
        if (Combo != null && ComboQuantity.HasValue)
        {
            total += Combo.Price * ComboQuantity.Value;
        }

        TotalPrice = total;
    }
}