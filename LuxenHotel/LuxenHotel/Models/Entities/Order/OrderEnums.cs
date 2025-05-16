namespace LuxenHotel.Models.Entities.Orders;

public enum OrderStatus
{
    Created,        // Order created
    Confirmed,      // Order confirmed
    InProgress,     // Service in progress
    Completed,      // Order completed
    Cancelled       // Order cancelled
}

public enum PaymentMethod
{
    Cash,
    CreditCard,
    VNPay,
    PayPal,
    MoMo,
    BankTransfer
}

public enum PaymentStatus
{
    Pending,        // Payment not initiated
    Processing,     // Payment being processed
    Completed,      // Payment successful
    Failed,         // Payment failed
    Refunded,       // Payment refunded
}