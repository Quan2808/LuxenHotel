// File: ServiceRegistration.cs
using LuxenHotel.Services; // Namespace chứa các service như IAccommodationService, IBookingService, ...
using Microsoft.AspNetCore.Identity;
using LuxenHotel.Data;
using LuxenHotel.Models.Entities.Booking;
using LuxenHotel.Models.Entities.Identity;
using LuxenHotel.Services.Booking.Implementations;
using LuxenHotel.Services.Booking.Interfaces;
using LuxenHotel.Services.Identity;

namespace LuxenHotel.Configuration
{
    public static class ServiceRegistration
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            // Register custom user store
            services.AddTransient<IUserStore<User>, CustomUserStore>();
            services.AddTransient<IRoleStore<Role>, CustomRoleStore>();

            // Register AccommodationService
            services.AddScoped<IAccommodationService, AccommodationService>();

            // Register ComboService
            services.AddScoped<IComboService, ComboService>();

            return services;
        }
    }
}