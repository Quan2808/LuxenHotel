using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations;

namespace LuxenHotel.Models.Entities.Order;

public class Payment
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int OrderId { get; set; }
    public Order Order { get; set; }

    // Transaction ID from third-party payment providers
    [Required]
    public string TransactionId { get; set; }

    [Required]
    public string PaymentProvider { get; set; } // VNPay, PayPal, MoMo, etc.

    [Required]
    public int Amount { get; set; }

    [Required]
    public PaymentStatus Status { get; set; } = PaymentStatus.Processing;

    // Response data from payment gateway
    public string? ResponseData { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}