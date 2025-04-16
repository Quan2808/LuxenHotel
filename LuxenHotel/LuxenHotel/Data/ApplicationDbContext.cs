using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LuxenHotel.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Rename Identity tables for clarity
            builder.Entity<IdentityUser>(entity => entity.ToTable("Users"));
            builder.Entity<IdentityRole>(entity => entity.ToTable("Roles"));
            builder.Entity<IdentityUserRole<string>>(entity => entity.ToTable("UserRoles"));

            // Ignore unused Identity entities
            builder.Ignore<IdentityUserClaim<string>>();
            builder.Ignore<IdentityUserLogin<string>>();
            builder.Ignore<IdentityUserToken<string>>();
            builder.Ignore<IdentityRoleClaim<string>>();

            // Configure IdentityUser entity
            builder.Entity<IdentityUser>(entity =>
            {
                // Required properties
                entity.Property(u => u.Id).IsRequired();
                entity.Property(u => u.UserName).IsRequired().HasMaxLength(256);
                entity.Property(u => u.NormalizedUserName).IsRequired().HasMaxLength(256);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
                entity.Property(u => u.NormalizedEmail).IsRequired().HasMaxLength(256);
                entity.Property(u => u.PhoneNumber).IsRequired().HasMaxLength(11);
                entity.Property(u => u.PasswordHash).HasColumnName("Password");
                entity.Property(u => u.SecurityStamp);
                entity.Property(u => u.ConcurrencyStamp);

                // Ignored properties
                entity.Ignore(u => u.PhoneNumberConfirmed);
                entity.Ignore(u => u.EmailConfirmed);
                entity.Ignore(u => u.LockoutEnabled);
                entity.Ignore(u => u.LockoutEnd);
                entity.Ignore(u => u.AccessFailedCount);
                entity.Ignore(u => u.TwoFactorEnabled);

                // Unique indexes
                entity.HasIndex(u => u.NormalizedUserName).IsUnique();
                entity.HasIndex(u => u.NormalizedEmail).IsUnique();
                entity.HasIndex(u => u.PhoneNumber).IsUnique();
            });
        }
    }
}