using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using LuxenHotel.Data;
using LuxenHotel.Models.Entities.Identity;
using LuxenHotel.Models.Entities.Orders;
using LuxenHotel.Models.ViewModels;
using LuxenHotel.Models.ViewModels.Booking;
using LuxenHotel.Services.Booking.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LuxenHotel.Areas.Customer.Controllers;

[Authorize]
[Area("Customer")]
public class OrdersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
     UserManager<User> userManager,
     ApplicationDbContext context,
     ILogger<OrdersController> logger)
    {
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [Route("Orders/MyOrders")]
    public async Task<IActionResult> MyOrders(string sortOrder, string searchString, int? pageNumber)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User ID claim not found for authenticated user.");
            return Unauthorized();
        }

        // Set up sorting
        ViewData["DateSortParam"] = string.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
        ViewData["StatusSortParam"] = sortOrder == "Status" ? "status_desc" : "Status";
        ViewData["PriceSortParam"] = sortOrder == "Price" ? "price_desc" : "Price";
        ViewData["CurrentFilter"] = searchString;

        // Get orders for the current user with all related entities
        var orders = _context.Orders
            .Include(o => o.Accommodation)
            .Include(o => o.OrderServices)
                .ThenInclude(os => os.Service)
            .Include(o => o.OrderCombos)
                .ThenInclude(oc => oc.Combo)
            .Where(o => o.UserId == userId);

        // Apply search filter if provided
        if (!string.IsNullOrEmpty(searchString))
        {
            orders = orders.Where(o =>
                o.OrderCode.Contains(searchString) ||
                o.Accommodation.Name.Contains(searchString) ||
                o.CustomerName.Contains(searchString) ||
                o.Status.ToString().Contains(searchString)
            );
        }

        // Apply sorting
        orders = sortOrder switch
        {
            "date_desc" => orders.OrderByDescending(o => o.CreatedAt),
            "Status" => orders.OrderBy(o => o.Status),
            "status_desc" => orders.OrderByDescending(o => o.Status),
            "Price" => orders.OrderBy(o => o.TotalPrice),
            "price_desc" => orders.OrderByDescending(o => o.TotalPrice),
            _ => orders.OrderBy(o => o.CreatedAt)
        };

        // Set page size
        int pageSize = 10;

        // Get user details for additional information
        var user = await _userManager.FindByIdAsync(userId);
        ViewData["UserFullName"] = user?.FullName;

        _logger.LogInformation("Retrieved {Count} orders for user {UserId}", await orders.CountAsync(), userId);

        return View(await PaginatedList<Orders>.CreateAsync(orders.AsNoTracking(), pageNumber ?? 1, pageSize));
    }

    // GET: Orders/Create/5
    [HttpGet]
    [Route("Orders/Create/{accommodationId:int}")]
    public async Task<IActionResult> Create(int accommodationId)
    {
        // Verify accommodation exists
        var accommodation = await _context.Accommodations
                .FirstOrDefaultAsync(a => a.Id == accommodationId);

        if (accommodation == null)
        {
            return NotFound();
        }
        var viewModel = new OrderCreateViewModel
        {
            AccommodationId = accommodationId,
            CheckInDate = DateTime.Today.AddDays(1),
            CheckOutDate = DateTime.Today.AddDays(2),
        };

        if (User.Identity.IsAuthenticated)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    viewModel.CustomerName = user.FullName;
                    viewModel.CustomerEmail = user.Email;
                    viewModel.CustomerPhone = user.PhoneNumber;
                }
                else
                {
                    // Log warning: User not found in database
                    _logger.LogWarning("User with ID {UserId} not found.", userId);
                }
            }
            else
            {
                // Log warning: User ID claim missing
                _logger.LogWarning("User ID claim not found for authenticated user.");
            }
        }

        ViewData["Accommodations"] = await _context.Accommodations
            .Where(a => a.Id == accommodationId)
            .AsNoTracking()
            .ToListAsync();
        ViewData["Services"] = await _context.Services
            .Where(s => s.AccommodationId == accommodationId)
            .ToListAsync();
        ViewData["Combos"] = await _context.Combos
            .Where(s => s.AccommodationId == accommodationId)
            .ToListAsync();

        return View(viewModel);
    }

    // POST: Orders/Create/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Orders/Create/{accommodationId:int}")]
    public async Task<IActionResult> Create(int accommodationId, OrderCreateViewModel viewModel, int[] selectedServiceIds, int[] serviceQuantities, int[] selectedComboIds, int[] comboQuantities)
    {
        // Ensure AccommodationId matches route parameter
        if (viewModel.AccommodationId != accommodationId)
        {
            ModelState.AddModelError("AccommodationId", "Invalid accommodation ID.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Fetch accommodation
                var accommodation = await _context.Accommodations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == accommodationId);

                if (accommodation == null)
                {
                    _logger.LogWarning("Accommodation with ID {AccommodationId} not found.", accommodationId);
                    ModelState.AddModelError("", "Accommodation not found.");
                    return View(viewModel);
                }

                // Map ViewModel to Orders entity
                var order = new Orders
                {
                    AccommodationId = viewModel.AccommodationId,
                    CustomerName = viewModel.CustomerName,
                    CustomerEmail = viewModel.CustomerEmail,
                    CustomerPhone = viewModel.CustomerPhone,
                    CheckInDate = viewModel.CheckInDate,
                    CheckOutDate = viewModel.CheckOutDate,
                    NumberOfGuests = viewModel.NumberOfGuests,
                    SpecialRequests = viewModel.SpecialRequests,
                    PaymentMethod = viewModel.PaymentMethod,
                    CreatedAt = DateTime.UtcNow,
                    Status = OrderStatus.Created,
                    PaymentStatus = PaymentStatus.Pending,
                    OrderServices = new List<OrderService>(),
                    OrderCombos = new List<OrderCombo>()
                };

                // Associate user if authenticated
                if (User.Identity.IsAuthenticated)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!string.IsNullOrEmpty(userId))
                    {
                        order.UserId = userId;
                    }
                }

                // Track prices
                Dictionary<int, int> servicePrices = new Dictionary<int, int>();
                Dictionary<int, int> comboPrices = new Dictionary<int, int>();

                // Add services
                if (selectedServiceIds != null && selectedServiceIds.Length > 0 && serviceQuantities != null && serviceQuantities.Length > 0)
                {
                    _logger.LogInformation("Processing {ServiceCount} selected services", selectedServiceIds.Length);
                    for (int i = 0; i < Math.Min(selectedServiceIds.Length, serviceQuantities.Length); i++)
                    {
                        if (selectedServiceIds[i] > 0 && serviceQuantities[i] > 0)
                        {
                            var service = await _context.Services
                                .AsNoTracking()
                                .FirstOrDefaultAsync(s => s.Id == selectedServiceIds[i]);

                            if (service != null)
                            {
                                var orderService = new OrderService
                                {
                                    ServiceId = selectedServiceIds[i],
                                    Quantity = serviceQuantities[i]
                                };
                                order.OrderServices.Add(orderService);
                                servicePrices[selectedServiceIds[i]] = service.Price;
                                _logger.LogInformation("Added service {ServiceId} with quantity {Quantity} to order",
                                    selectedServiceIds[i], serviceQuantities[i]);
                            }
                            else
                            {
                                _logger.LogWarning("Service with ID {ServiceId} not found.", selectedServiceIds[i]);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Invalid service data at index {Index}: ServiceId={ServiceId}, Quantity={Quantity}",
                                i, selectedServiceIds[i], serviceQuantities[i]);
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("No valid services selected or quantities provided.");
                }

                // Add combos
                if (selectedComboIds != null && selectedComboIds.Length > 0 && comboQuantities != null && comboQuantities.Length > 0)
                {
                    _logger.LogInformation("Processing {ComboCount} selected combos", selectedComboIds.Length);
                    for (int i = 0; i < Math.Min(selectedComboIds.Length, comboQuantities.Length); i++)
                    {
                        if (selectedComboIds[i] > 0 && comboQuantities[i] > 0)
                        {
                            var combo = await _context.Combos
                                .AsNoTracking()
                                .FirstOrDefaultAsync(c => c.Id == selectedComboIds[i]);

                            if (combo != null)
                            {
                                var orderCombo = new OrderCombo
                                {
                                    ComboId = selectedComboIds[i],
                                    Quantity = comboQuantities[i]
                                };
                                order.OrderCombos.Add(orderCombo);
                                comboPrices[selectedComboIds[i]] = combo.Price;
                                _logger.LogInformation("Added combo {ComboId} with quantity {Quantity} to order",
                                    selectedComboIds[i], comboQuantities[i]);
                            }
                            else
                            {
                                _logger.LogWarning("Combo with ID {ComboId} not found.", selectedComboIds[i]);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Invalid combo data at index {Index}: ComboId={ComboId}, Quantity={Quantity}",
                                i, selectedComboIds[i], comboQuantities[i]);
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("No valid combos selected or quantities provided.");
                }

                // Generate order code
                order.OrderCode = Guid.NewGuid().ToString("N")[..10].ToUpper();

                // Calculate total price
                int accommodationPriceForCalculation = accommodation.Price;
                int numberOfDays = Math.Max(1, (int)Math.Ceiling((order.CheckOutDate - order.CheckInDate).TotalDays));
                int totalPrice = accommodationPriceForCalculation * numberOfDays;

                foreach (var orderService in order.OrderServices)
                {
                    if (servicePrices.TryGetValue(orderService.ServiceId, out int servicePrice))
                    {
                        totalPrice += orderService.Quantity * servicePrice;
                    }
                }

                foreach (var orderCombo in order.OrderCombos)
                {
                    if (comboPrices.TryGetValue(orderCombo.ComboId, out int comboPrice))
                    {
                        totalPrice += orderCombo.Quantity * comboPrice;
                    }
                }

                order.TotalPrice = totalPrice;
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully created order with ID {OrderId} containing {ServiceCount} services and {ComboCount} combos",
                    order.Id, order.OrderServices.Count, order.OrderCombos.Count);

                return RedirectToAction(nameof(Details), new { id = order.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order for AccommodationId {AccommodationId}.", accommodationId);
                ModelState.AddModelError("", $"An error occurred while creating the order: {ex.Message}");
            }
        }

        // Reload dropdown data if validation fails
        ViewData["Accommodations"] = await _context.Accommodations
            .Where(a => a.Id == accommodationId)
            .AsNoTracking()
            .ToListAsync();
        ViewData["Services"] = await _context.Services
            .Where(s => s.AccommodationId == accommodationId)
            .AsNoTracking()
            .ToListAsync();
        ViewData["Combos"] = await _context.Combos
            .Where(c => c.AccommodationId == accommodationId)
            .AsNoTracking()
            .ToListAsync();

        return View(viewModel);
    }

    // GET: Orders/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var order = await _context.Orders
            .Include(o => o.Accommodation)
            .Include(o => o.OrderServices)
                .ThenInclude(os => os.Service)
            .Include(o => o.OrderCombos)
                .ThenInclude(oc => oc.Combo)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }


    // GET: Orders/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var order = await _context.Orders
            .Include(o => o.OrderServices)
            .Include(o => o.OrderCombos)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        ViewData["Accommodations"] = _context.Accommodations.ToList();
        ViewData["Services"] = _context.Services.ToList();
        ViewData["Combos"] = _context.Combos.ToList();
        return View(order);
    }

    // POST: Orders/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Orders order, int[] selectedServiceIds, int[] serviceQuantities,
        int[] selectedComboIds, int[] comboQuantities)
    {
        if (id != order.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                var existingOrder = await _context.Orders
                    .Include(o => o.OrderServices)
                    .Include(o => o.OrderCombos)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (existingOrder == null)
                {
                    return NotFound();
                }

                // Update basic properties
                existingOrder.CheckInDate = order.CheckInDate;
                existingOrder.CheckOutDate = order.CheckOutDate;
                existingOrder.NumberOfGuests = order.NumberOfGuests;
                existingOrder.SpecialRequests = order.SpecialRequests;
                existingOrder.CustomerName = order.CustomerName;
                existingOrder.CustomerEmail = order.CustomerEmail;
                existingOrder.CustomerPhone = order.CustomerPhone;
                existingOrder.AccommodationId = order.AccommodationId;
                existingOrder.PaymentMethod = order.PaymentMethod;
                existingOrder.Status = order.Status;
                existingOrder.PaymentStatus = order.PaymentStatus;
                existingOrder.UpdatedAt = DateTime.UtcNow;

                // Update services
                existingOrder.OrderServices.Clear();
                if (selectedServiceIds != null && selectedServiceIds.Length > 0)
                {
                    for (int i = 0; i < selectedServiceIds.Length; i++)
                    {
                        if (selectedServiceIds[i] > 0 && serviceQuantities[i] > 0)
                        {
                            var service = await _context.Services.FindAsync(selectedServiceIds[i]);
                            if (service != null)
                            {
                                existingOrder.OrderServices.Add(new OrderService
                                {
                                    ServiceId = selectedServiceIds[i],
                                    Quantity = serviceQuantities[i],
                                    Service = service
                                });
                            }
                        }
                    }
                }

                // Update combos
                existingOrder.OrderCombos.Clear();
                if (selectedComboIds != null && selectedComboIds.Length > 0)
                {
                    for (int i = 0; i < selectedComboIds.Length; i++)
                    {
                        if (selectedComboIds[i] > 0 && comboQuantities[i] > 0)
                        {
                            var combo = await _context.Combos.FindAsync(selectedComboIds[i]);
                            if (combo != null)
                            {
                                existingOrder.OrderCombos.Add(new OrderCombo
                                {
                                    ComboId = selectedComboIds[i],
                                    Quantity = comboQuantities[i],
                                    Combo = combo
                                });
                            }
                        }
                    }
                }

                // Recalculate total price
                existingOrder.Accommodation = await _context.Accommodations.FindAsync(existingOrder.AccommodationId);
                existingOrder.CalculateTotalPrice();

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(order.Id))
                {
                    return NotFound();
                }

                throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred while updating the order: {ex.Message}");
            }
        }

        // Reload dropdown data if validation fails
        ViewData["Accommodations"] = _context.Accommodations.ToList();
        ViewData["Services"] = _context.Services.ToList();
        ViewData["Combos"] = _context.Combos.ToList();
        return View(order);
    }

    // GET: Orders/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Accommodation)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }

    // POST: Orders/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"An error occurred while deleting the order: {ex.Message}");
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Orders/Cancel/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string cancellationReason)
    {
        try
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = OrderStatus.Cancelled;
            order.CancellationReason = cancellationReason;
            order.CancelledAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"An error occurred while cancelling the order: {ex.Message}");
            return RedirectToAction(nameof(Index));
        }
    }

    private bool OrderExists(int id)
    {
        return _context.Orders.Any(e => e.Id == id);
    }
}