using LuxenHotel.Models.Entities.Booking;
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

        // DbSets from BookingContext
        public DbSet<Accommodation> Accommodations { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Combo> Combos { get; set; }
        public DbSet<ComboService> ComboServices { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Call the separated configuration methods
            ConfigureIdentity(builder);
            ConfigureBooking(builder);
        }

        private void ConfigureIdentity(ModelBuilder builder)
        {
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

        private void ConfigureBooking(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Service>()
                .HasOne(s => s.Accommodation)
                .WithMany(a => a.Services)
                .HasForeignKey(s => s.AccommodationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ComboService>()
                .HasKey(cs => new { cs.ComboId, cs.ServiceId });

            modelBuilder.Entity<ComboService>()
                .HasOne(cs => cs.Combo)
                .WithMany(c => c.ComboServices)
                .HasForeignKey(cs => cs.ComboId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ComboService>()
                .HasOne(cs => cs.Service)
                .WithMany(s => s.ComboServices)
                .HasForeignKey(cs => cs.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}