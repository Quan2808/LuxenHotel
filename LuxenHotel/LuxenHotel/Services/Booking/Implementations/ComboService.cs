using LuxenHotel.Data;
using LuxenHotel.Models.Entities.Booking;
using LuxenHotel.Models.ViewModels.Booking;
using LuxenHotel.Services.Booking.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LuxenHotel.Services.Booking.Implementations;

public class ComboService : IComboService
{
    private readonly ApplicationDbContext _context;

    public ComboService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ComboViewModel>> ListAsync()
    {
        // Fetch combos with their accommodations
        var combos = await _context.Combos
            .AsNoTracking()
            .Include(c => c.Accommodation)
            .Include(c => c.ComboServices) // Direct include for the many-to-many relationship
            .ToListAsync();

        // Map to view models
        var result = combos.Select(combo => new ComboViewModel
        {
            Id = combo.Id,
            Name = combo.Name,
            Price = combo.Price,
            Description = combo.Description,
            AccommodationId = combo.AccommodationId,
            AccommodationName = combo.Accommodation.Name,
            Status = combo.Status,
            CreatedAt = combo.CreatedAt,
            Services = combo.ComboServices.Select(service => new ServiceViewModel
            {
                Id = service.Id,
                Name = service.Name
            })
        }).ToList();

        return result;
    }

    public async Task<List<Combo>> GetCombosByAccommodationIdAsync(int accommodationId)
    {
        return await _context.Combos
            .AsNoTracking()
            .Where(c => c.AccommodationId == accommodationId)
            .Include(c => c.Accommodation)
            .Include(c => c.ComboServices)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<Combo?> GetComboByIdAsync(int comboId)
    {
        return await _context.Combos
            .AsNoTracking()
            .Include(c => c.Accommodation)
            .Include(c => c.ComboServices)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == comboId);
    }

    public async Task<Combo> CreateComboAsync(Combo combo, List<int> selectedServiceIds)
    {
        combo.CreatedAt = DateTime.UtcNow;
        // combo.UpdatedAt = DateTime.UtcNow;

        // First, save the combo without services to get an ID
        await _context.Combos.AddAsync(combo);
        await _context.SaveChangesAsync();

        // Now handle the many-to-many relationship
        if (selectedServiceIds != null && selectedServiceIds.Any())
        {
            // Get the actual service entities from the database
            var services = await _context.Services
                .Where(s => selectedServiceIds.Contains(s.Id))
                .ToListAsync();

            // Clear any existing relationships
            combo.ComboServices = new List<Service>();

            // Add each service to the combo's ComboServices collection
            foreach (var service in services)
            {
                combo.ComboServices.Add(service);
            }

            // Save the changes
            await _context.SaveChangesAsync();
        }

        return combo;
    }

    public async Task<Combo?> UpdateComboAsync(int comboId, Combo updatedCombo)
    {
        var existingCombo = await _context.Combos
            .Include(c => c.ComboServices)
            .FirstOrDefaultAsync(c => c.Id == comboId);

        if (existingCombo == null)
        {
            return null;
        }

        existingCombo.Name = updatedCombo.Name;
        existingCombo.Price = updatedCombo.Price;
        existingCombo.Description = updatedCombo.Description;
        existingCombo.Status = updatedCombo.Status;
        existingCombo.AccommodationId = updatedCombo.AccommodationId;
        existingCombo.UpdatedAt = DateTime.UtcNow;

        if (updatedCombo.ComboServices != null)
        {
            var currentServiceIds = existingCombo.ComboServices.Select(s => s.Id).ToList();
            var updatedServiceIds = updatedCombo.ComboServices.Select(s => s.Id).ToList();

            var servicsToRemove = existingCombo.ComboServices
                .Where(s => !updatedServiceIds.Contains(s.Id))
                .ToList();

            foreach (var service in servicsToRemove)
            {
                existingCombo.ComboServices.Remove(service);
            }

            var existingServiceIds = existingCombo.ComboServices.Select(s => s.Id).ToList();
            var servicesToAdd = updatedCombo.ComboServices
                .Where(s => !existingServiceIds.Contains(s.Id))
                .ToList();

            foreach (var service in servicesToAdd)
            {
                existingCombo.ComboServices.Add(service);
            }
        }

        await _context.SaveChangesAsync();
        return existingCombo;
    }

    public async Task<bool> DeleteComboAsync(int comboId)
    {
        var combo = await _context.Combos.FindAsync(comboId);
        if (combo == null)
        {
            return false;
        }

        _context.Combos.Remove(combo);
        await _context.SaveChangesAsync();
        return true;
    }

}