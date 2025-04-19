using LuxenHotel.Data;
using LuxenHotel.Models.Entities.Identity;
using LuxenHotel.Services.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

            // Register custom user store
            services.AddTransient<IUserStore<User>, CustomUserStore>();
            services.AddTransient<IRoleStore<Role>, CustomRoleStore>();

            // services.AddScoped<AccommodationStore>();

            // Configure Identity
            services.AddIdentity<User, Role>(options =>
            {
                // Configure Identity options as needed
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;

                // Disable features we don't need
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
                options.Lockout.AllowedForNewUsers = false;
            })
            // .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

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
                // app.UseExceptionHandler("/Error");
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An unexpected error occurred. Please try again later.");
                    });
                });
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // Seed data
            SeedData.InitializeAsync(app.ApplicationServices).GetAwaiter().GetResult();

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