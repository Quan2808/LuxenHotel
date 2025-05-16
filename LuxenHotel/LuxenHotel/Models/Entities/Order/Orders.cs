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
    public string OrderCode { get; set; } = Guid.NewGuid().ToString("N")[..10].ToUpper();

    public string? UserId { get; set; }
    public User? User { get; set; }

    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }

    [Required]
    public int AccommodationId { get; set; }
    public Accommodation Accommodation { get; set; }

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

    [Required]
    public DateTime CheckInDate { get; set; }

    [Required]
    public DateTime CheckOutDate { get; set; }

    [Required]
    [Range(1, 50, ErrorMessage = "Number of guests must be between 1 and 50")]
    public int NumberOfGuests { get; set; }

    public string? SpecialRequests { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    public string? CancellationReason { get; set; }

    // Danh sách dịch vụ & combo
    public List<OrderService> OrderServices { get; set; } = new();
    public List<OrderCombo> OrderCombos { get; set; } = new();

    public void CalculateTotalPrice()
    {
        int total = Accommodation?.Price ?? 0;

        int numberOfDays = (int)Math.Ceiling((CheckOutDate - CheckInDate).TotalDays);
        if (numberOfDays < 1) numberOfDays = 1;
        total *= numberOfDays;

        foreach (var os in OrderServices)
        {
            total += os.Service?.Price * os.Quantity ?? 0;
        }

        foreach (var oc in OrderCombos)
        {
            total += oc.Combo?.Price * oc.Quantity ?? 0;
        }

        TotalPrice = total;
    }
}

public class OrderService
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int OrderId { get; set; }
    public Orders Order { get; set; }

    [Required]
    public int ServiceId { get; set; }
    public Service Service { get; set; }

    [Required]
    [Range(1, 100, ErrorMessage = "Số lượng phải lớn hơn 0")]
    public int Quantity { get; set; }
}


public class OrderCombo
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int OrderId { get; set; }
    public Orders Order { get; set; }

    [Required]
    public int ComboId { get; set; }
    public Combo Combo { get; set; }

    [Required]
    [Range(1, 100, ErrorMessage = "Số lượng phải lớn hơn 0")]
    public int Quantity { get; set; }
}
