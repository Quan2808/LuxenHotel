using LuxenHotel.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace LuxenHotel
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Configure DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // Configure Identity
            services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;

                options.Lockout.AllowedForNewUsers = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // Configure Claims Identity
            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserIdClaimType = null;
                options.ClaimsIdentity.UserNameClaimType = null;
                options.ClaimsIdentity.RoleClaimType = null;
            });

            // Configure routing
            services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
                options.LowercaseQueryStrings = true;
            });

            // Configure authentication cookies
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                // options.AccessDeniedPath = "/Account/AccessDenied";
            });

            services.Configure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, options =>
            {
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            // Configure Logging
            services.AddLogging(logging =>
            {
                logging.AddConsole();
                logging.AddDebug();
                logging.SetMinimumLevel(LogLevel.Debug);
            });

            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (!env.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // Seed data
            using (var scope = app.ApplicationServices.CreateScope())
            {
                SeedData.InitializeAsync(scope.ServiceProvider).GetAwaiter().GetResult();
            }

            app.UseEndpoints(endpoints =>
            {
                // Customer Area: Short route for HomeController
                endpoints.MapAreaControllerRoute(
                    name: "customer_pages",
                    areaName: "Customer",
                    pattern: "{action=Index}/{id?}",
                    defaults: new { controller = "Home" });

                // Customer Area: Default route
                endpoints.MapAreaControllerRoute(
                    name: "customer_area",
                    areaName: "Customer",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                // Admin Area
                endpoints.MapAreaControllerRoute(
                    name: "admin_area",
                    areaName: "Admin",
                    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}");

                // Staff Area
                endpoints.MapAreaControllerRoute(
                    name: "staff_area",
                    areaName: "Staff",
                    pattern: "Staff/{controller=Task}/{action=Index}/{id?}");

                // Identity Area
                endpoints.MapAreaControllerRoute(
                    name: "identity_area",
                    areaName: "Identity",
                    pattern: "Identity/{controller=Account}/{action=Login}/{id?}");
            });
        }
    }
}