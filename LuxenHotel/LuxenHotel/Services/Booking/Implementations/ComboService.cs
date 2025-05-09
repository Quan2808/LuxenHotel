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

    public async Task<List<AccommodationViewModel>> ListAsync()
    {
        var accommodations = await _context.Accommodations
            .AsNoTracking()
            .Include(a => a.Services)
            .Include(a => a.Combos)
                .ThenInclude(c => c.ComboServices)
            .Where(a => a.Combos.Any())
            .ToListAsync();

        return accommodations.Select(ToViewModel).ToList();
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

    private AccommodationViewModel ToViewModel(Accommodation accommodation) => new()
    {
        Id = accommodation.Id,
        Name = accommodation.Name,
        Thumbnail = accommodation.Thumbnail,
        Services = accommodation.Services?.Select(s => new ServiceViewModel
        {
            Id = s.Id,
            Name = s.Name,
            Price = s.Price,
            Description = s.Description
        }).ToList() ?? new List<ServiceViewModel>(),
        Combos = accommodation.Combos?.Select(c => new ComboViewModel
        {
            Id = c.Id,
            Name = c.Name,
            Price = c.Price,
            Description = c.Description,
            Status = c.Status,
            CreatedAt = c.CreatedAt,
            SelectedServiceIds = c.ComboServices?.Select(s => s.Id).ToList() ?? new List<int>()
        }).ToList() ?? new List<ComboViewModel>()
    };
}