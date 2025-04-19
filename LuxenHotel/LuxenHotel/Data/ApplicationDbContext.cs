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
        public DbSet<AccommodationService> AccommodationServices { get; set; }

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

        private void ConfigureBooking(ModelBuilder builder)
        {
            // Configure composite keys
            builder.Entity<ComboService>()
                .HasKey(cs => new { cs.ComboId, cs.ServiceId });

            builder.Entity<AccommodationService>()
                .HasKey(acs => new { acs.AccommodationId, acs.ServiceId });

            // Configure unique constraints
            builder.Entity<Accommodation>()
                .HasIndex(a => a.Name)
                .IsUnique();

            builder.Entity<Service>()
                .HasIndex(s => s.Name)
                .IsUnique();

            builder.Entity<Combo>()
                .HasIndex(c => c.Name)
                .IsUnique();

            // Configure relationships
            builder.Entity<Combo>()
                .HasOne(c => c.Accommodation)
                .WithMany(a => a.Combos)
                .HasForeignKey(c => c.AccommodationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ComboService>()
                .HasOne(cs => cs.Combo)
                .WithMany(c => c.ComboServices)
                .HasForeignKey(cs => cs.ComboId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ComboService>()
                .HasOne(cs => cs.Service)
                .WithMany(s => s.ComboServices)
                .HasForeignKey(cs => cs.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AccommodationService>()
                .HasOne(acs => acs.Accommodation)
                .WithMany(a => a.AccommodationServices)
                .HasForeignKey(acs => acs.AccommodationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AccommodationService>()
                .HasOne(acs => acs.Service)
                .WithMany(s => s.AccommodationServices)
                .HasForeignKey(acs => acs.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}