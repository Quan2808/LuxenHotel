using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LuxenHotel.Data
{
    /// <summary>
    /// Class to seed initial data such as roles and admin user.
    /// </summary>
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Seed roles
                string[] roles = { "Admin", "Staff", "Customer" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        var roleResult = await roleManager.CreateAsync(new IdentityRole(role));
                        if (!roleResult.Succeeded)
                        {
                            throw new Exception($"Failed to create role {role}: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                        }
                    }
                }

                // Seed admin user
                var adminUser = new IdentityUser
                {
                    UserName = "admin",
                    NormalizedUserName = "ADMIN",
                    Email = "admin@example.com",
                    NormalizedEmail = "ADMIN@EXAMPLE.COM",
                    PhoneNumber = "12345678901"
                };

                // Check if user exists using Email (avoid NormalizedUserName)
                var existingUser = await dbContext.Users
                    .FirstOrDefaultAsync(u => u.NormalizedEmail == adminUser.NormalizedEmail);

                if (existingUser == null)
                {
                    var userResult = await userManager.CreateAsync(adminUser, "Admin@123456");
                    if (!userResult.Succeeded)
                    {
                        throw new Exception($"Failed to create admin user: {string.Join(", ", userResult.Errors.Select(e => e.Description))}");
                    }

                    var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                    if (!roleResult.Succeeded)
                    {
                        throw new Exception($"Failed to assign Admin role to user: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error (in a real app, use a logging framework like Serilog)
                Console.WriteLine($"Error during seeding: {ex.Message}");
                throw;
            }
        }
    }
}