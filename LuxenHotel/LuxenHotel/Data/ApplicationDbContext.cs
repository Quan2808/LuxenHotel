using LuxenHotel.Models.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LuxenHotel.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Rename Identity tables for clarity
            builder.Entity<User>().ToTable("Users");
            builder.Entity<Role>().ToTable("Roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");

            // Ignore unused Identity entities
            builder.Entity<IdentityUserClaim<string>>().ToTable("AspNetUserClaims", table => table.ExcludeFromMigrations());
            builder.Entity<IdentityUserLogin<string>>().ToTable("AspNetUserLogins", table => table.ExcludeFromMigrations());
            builder.Entity<IdentityUserToken<string>>().ToTable("AspNetUserTokens", table => table.ExcludeFromMigrations());
            builder.Entity<IdentityRoleClaim<string>>().ToTable("AspNetRoleClaims", table => table.ExcludeFromMigrations());

            // Configure IdentityUser entity
            builder.Entity<User>(entity =>
            {
                // Required properties
                entity.Property(u => u.PasswordHash).HasColumnName("Password");

                // Ignored properties
                entity.Ignore(e => e.PhoneNumberConfirmed);
                entity.Ignore(e => e.EmailConfirmed);
                entity.Ignore(e => e.LockoutEnabled);
                entity.Ignore(e => e.LockoutEnd);
                entity.Ignore(e => e.AccessFailedCount);
                entity.Ignore(e => e.TwoFactorEnabled);
            });
        }
    }
}