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
            .Where(c => c.AccommodationId == accommodationId)
            .Include(c => c.Accommodation)
            .Include(c => c.ComboServices)
            .ToListAsync();
    }

    public async Task<Combo?> GetComboByIdAsync(int comboId)
    {
        return await _context.Combos
            .Include(c => c.Accommodation)
            .Include(c => c.ComboServices)
            .FirstOrDefaultAsync(c => c.Id == comboId);
    }

    public async Task<Combo> CreateComboAsync(Combo combo)
    {
        combo.CreatedAt = DateTime.UtcNow;
        _context.Combos.Add(combo);
        await _context.SaveChangesAsync();
        return combo;
    }

    public async Task<Combo?> UpdateComboAsync(int comboId, Combo combo)
    {
        var existingCombo = await _context.Combos.FindAsync(comboId);
        if (existingCombo == null)
        {
            return null;
        }

        existingCombo.Name = combo.Name;
        existingCombo.Price = combo.Price;
        existingCombo.Description = combo.Description;
        existingCombo.Status = combo.Status;
        existingCombo.AccommodationId = combo.AccommodationId;
        existingCombo.UpdatedAt = DateTime.UtcNow;

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

    private ComboViewModel ToViewModel(Combo combo)
    {
        return new ComboViewModel
        {
            Id = combo.Id,
            Name = combo.Name,
            Price = combo.Price,
            Description = combo.Description,
            AccommodationId = combo.AccommodationId,
            AccommodationName = combo.Accommodation.Name,
            Status = combo.Status,
            CreatedAt = combo.CreatedAt,
            Services = combo.ComboServices
                .Select(cs => new ServiceViewModel
                {
                    Id = cs.Id,
                    Name = cs.Name
                })
        };
    }
}