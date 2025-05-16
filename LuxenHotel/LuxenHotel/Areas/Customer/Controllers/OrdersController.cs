using System.ComponentModel.DataAnnotations;
using LuxenHotel.Data;
using LuxenHotel.Models.Entities.Orders;
using LuxenHotel.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LuxenHotel.Areas.Customer.Controllers;

[Area("Customer")]
public class OrdersController : Controller
{
    private readonly ApplicationDbContext _context;

    public OrdersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Orders/Create/5
        [HttpGet]
        [Route("Orders/Create/{accommodationId:int}")]
        public async Task<IActionResult> Create(int accommodationId)
        {
            // Verify accommodation exists
            var accommodation = await _context.Accommodations.FindAsync(accommodationId);
            if (accommodation == null)
            {
                return NotFound();
            }

            var viewModel = new OrderCreateViewModel
            {
                AccommodationId = accommodationId
            };

            ViewData["Accommodations"] = await _context.Accommodations.ToListAsync();
            ViewData["Services"] = await _context.Services.ToListAsync();
            ViewData["Combos"] = await _context.Combos.ToListAsync();
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
                        PaymentStatus = PaymentStatus.Pending
                    };

                    // Fetch accommodation for price calculation
                    order.Accommodation = await _context.Accommodations.FindAsync(order.AccommodationId);
                    if (order.Accommodation == null)
                    {
                        return NotFound();
                    }

                    // Add services
                    if (selectedServiceIds != null && selectedServiceIds.Length > 0)
                    {
                        for (int i = 0; i < selectedServiceIds.Length; i++)
                        {
                            if (selectedServiceIds[i] > 0 && serviceQuantities[i] > 0)
                            {
                                var service = await _context.Services.FindAsync(selectedServiceIds[i]);
                                if (service != null)
                                {
                                    order.OrderServices.Add(new OrderService
                                    {
                                        ServiceId = selectedServiceIds[i],
                                        Quantity = serviceQuantities[i],
                                        Service = service
                                    });
                                }
                            }
                        }
                    }

                    // Add combos
                    if (selectedComboIds != null && selectedComboIds.Length > 0)
                    {
                        for (int i = 0; i < selectedComboIds.Length; i++)
                        {
                            if (selectedComboIds[i] > 0 && comboQuantities[i] > 0)
                            {
                                var combo = await _context.Combos.FindAsync(selectedComboIds[i]);
                                if (combo != null)
                                {
                                    order.OrderCombos.Add(new OrderCombo
                                    {
                                        ComboId = selectedComboIds[i],
                                        Quantity = comboQuantities[i],
                                        Combo = combo
                                    });
                                }
                            }
                        }
                    }

                    // Calculate total price
                    order.CalculateTotalPrice();

                    // Add to context and save
                    _context.Add(order);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An error occurred while creating the order: {ex.Message}");
                }
            }

            // Reload dropdown data if validation fails
            ViewData["Accommodations"] = await _context.Accommodations.ToListAsync();
            ViewData["Services"] = await _context.Services.ToListAsync();
            ViewData["Combos"] = await _context.Combos.ToListAsync();
            return View(viewModel);
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