using LuxenHotel.Data;
using LuxenHotel.Models.Entities.Booking;
using LuxenHotel.Models.Entities.Orders;
using LuxenHotel.Models.ViewModels.Booking;
using LuxenHotel.Models.ViewModels.Orders;
using LuxenHotel.Services.Booking.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LuxenHotel.Areas.Admin.Controllers;

[Route("admin/orders")]
public class OrderManagerController : AdminBaseController
{
    private readonly ApplicationDbContext _context;

    public OrderManagerController(
        ILogger<AdminBaseController> logger,
        ApplicationDbContext context
    ) : base(logger)
    {
        _context = context;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        SetPageTitle("Orders Management");
        var orders = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Accommodation)
            .ToListAsync();
        return View(orders);
    }

    [HttpGet("{orderCode}")]
    public async Task<IActionResult> Details(string orderCode)
    {
        if (string.IsNullOrEmpty(orderCode))
        {
            return BadRequest("Order code is required.");
        }

        var order = await _context.Orders
            .Include(o => o.Accommodation)
            .Include(o => o.User)
            .Include(o => o.OrderServices)
                .ThenInclude(os => os.Service)
            .Include(o => o.OrderCombos)
                .ThenInclude(oc => oc.Combo)
            .FirstOrDefaultAsync(o => o.OrderCode == orderCode);

        if (order == null)
        {
            return NotFound($"No order found with code {orderCode}.");
        }

        var viewModel = new OrderDetailsViewModel
        {
            OrderCode = order.OrderCode,
            CustomerName = order.CustomerName ?? order.User?.UserName ?? "N/A",
            CustomerEmail = order.CustomerEmail ?? "N/A",
            CustomerPhone = order.CustomerPhone ?? "N/A",
            AccommodationName = order.Accommodation?.Name ?? "N/A",
            TotalPrice = order.TotalPrice,
            PaymentStatus = order.PaymentStatus.ToString(),
            OrderStatus = order.Status.ToString(),
            CheckInDate = order.CheckInDate,
            CheckOutDate = order.CheckOutDate,
            NumberOfGuests = order.NumberOfGuests,
            SpecialRequests = order.SpecialRequests ?? "None",
            CreatedAt = order.CreatedAt,
            Services = order.OrderServices.Select(os => new OrderServiceViewModel
            {
                ServiceName = os.Service?.Name ?? "N/A",
                Quantity = os.Quantity,
                Price = os.Service?.Price ?? 0
            }).ToList(),
            Combos = order.OrderCombos.Select(oc => new OrderComboViewModel
            {
                ComboName = oc.Combo?.Name ?? "N/A",
                Quantity = oc.Quantity,
                Price = oc.Combo?.Price ?? 0
            }).ToList()
        };
        ViewBag.AccommodationPrice = order.Accommodation?.Price ?? 0;
        SetPageTitle($"Order Details - {order.OrderCode}");
        return View(viewModel);
    }
}