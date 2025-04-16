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

            builder.Entity<IdentityUser>(entity => entity.ToTable("Users"));
            builder.Entity<IdentityRole>(entity => entity.ToTable("Roles"));
            builder.Entity<IdentityUserRole<string>>(entity => entity.ToTable("UserRoles"));

            builder.Ignore<IdentityUserClaim<string>>();
            builder.Ignore<IdentityUserLogin<string>>();
            builder.Ignore<IdentityUserToken<string>>();
            builder.Ignore<IdentityRoleClaim<string>>();

            builder.Entity<IdentityUser>(entity =>
            {
                entity.Property(u => u.Id).IsRequired();
                entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
                entity.Property(u => u.NormalizedEmail).IsRequired().HasMaxLength(256);
                entity.Property(u => u.PhoneNumber).IsRequired().HasMaxLength(11);
                entity.Property(u => u.PasswordHash).HasColumnName("Password");
                entity.Property(u => u.SecurityStamp);
                entity.Property(u => u.ConcurrencyStamp);

                entity.Ignore(u => u.UserName);
                entity.Ignore(u => u.NormalizedUserName);
                entity.Ignore(u => u.PhoneNumberConfirmed);
                entity.Ignore(u => u.EmailConfirmed);
                entity.Ignore(u => u.LockoutEnabled);
                entity.Ignore(u => u.LockoutEnd);
                entity.Ignore(u => u.AccessFailedCount);
                entity.Ignore(u => u.TwoFactorEnabled);

                entity.HasIndex(u => u.NormalizedEmail).IsUnique();
                entity.HasIndex(u => u.PhoneNumber).IsUnique();
            });
        }
    }
}