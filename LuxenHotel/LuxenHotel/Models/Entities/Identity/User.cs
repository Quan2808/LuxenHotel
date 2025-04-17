using Microsoft.AspNetCore.Identity;

namespace LuxenHotel.Models.Entities.Identity;
public class User : IdentityUser
{
    public string? FullName { get; set; }

    // We'll override the base properties that we want to remove
    // These won't be used in our implementation
    public override bool PhoneNumberConfirmed { get; set; } // Will be ignored
    public override bool EmailConfirmed { get; set; } // Will be ignored
    public override bool LockoutEnabled { get; set; } // Will be ignored
    public override DateTimeOffset? LockoutEnd { get; set; } // Will be ignored
    public override int AccessFailedCount { get; set; } // Will be ignored
    public override bool TwoFactorEnabled { get; set; } // Will be ignored
}

