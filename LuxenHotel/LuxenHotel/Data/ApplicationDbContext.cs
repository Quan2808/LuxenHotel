using LuxenHotel.Models.Entities.Booking;
using LuxenHotel.Models.Entities.Identity;
using LuxenHotel.Models.Entities.Order;
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
        
        public DbSet<Order> Orders { get; set; }
        public DbSet<Payment> Payments { get; set; }

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

            modelBuilder.Entity<Combo>()
                .HasMany(c => c.ComboServices)
                .WithMany(s => s.ComboServices)
                .UsingEntity<Dictionary<string, object>>(
                    j => j
                        .HasOne<Service>()
                        .WithMany()
                        .HasForeignKey("ServiceId")
                        .OnDelete(DeleteBehavior.Restrict),
                    j => j
                        .HasOne<Combo>()
                        .WithMany()
                        .HasForeignKey("ComboId")
                        .OnDelete(DeleteBehavior.Restrict)
                ).ToTable("ComboService");
        }
        
        private void ConfigureOrder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .IsRequired(false) 
                .OnDelete(DeleteBehavior.SetNull);
                
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Accommodation)
                .WithMany()
                .HasForeignKey(o => o.AccommodationId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Service)
                .WithMany()
                .HasForeignKey(o => o.ServiceId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Combo)
                .WithMany()
                .HasForeignKey(o => o.ComboId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<Order>()
                .ToTable(t => t.HasCheckConstraint("CK_Order_ServiceOrCombo", 
                    "([ServiceId] IS NULL AND [ComboId] IS NOT NULL) OR ([ServiceId] IS NOT NULL AND [ComboId] IS NULL)"));
                
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithMany()
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade); 
                
            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.TransactionId)
                .IsUnique();
                
            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasConversion<string>();
                
            modelBuilder.Entity<Order>()
                .Property(o => o.PaymentMethod)
                .HasConversion<string>();
                
            modelBuilder.Entity<Order>()
                .Property(o => o.PaymentStatus)
                .HasConversion<string>();
                
            modelBuilder.Entity<Payment>()
                .Property(p => p.Status)
                .HasConversion<string>();
                
            modelBuilder.Entity<Order>()
                .Property(o => o.OrderCode)
                .IsRequired()
                .HasMaxLength(20);
                
            modelBuilder.Entity<Payment>()
                .Property(p => p.TransactionId)
                .IsRequired()
                .HasMaxLength(100);
                
            modelBuilder.Entity<Payment>()
                .Property(p => p.PaymentProvider)
                .IsRequired()
                .HasMaxLength(50);
        }   
    }
}