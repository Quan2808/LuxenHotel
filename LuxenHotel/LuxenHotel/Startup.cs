using Microsoft.EntityFrameworkCore;
using LuxenHotel.Data;

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
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // services.AddIdentity<IdentityUser, IdentityRole>()
            //     .AddEntityFrameworkStores<ApplicationDbContext>()
            //     .AddDefaultTokenProviders();

            services.AddControllersWithViews();

            // Cấu hình RouteOptions để bỏ qua phân biệt chữ hoa/thường
            services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true; // Chuyển URL thành chữ thường
                options.LowercaseQueryStrings = true; // (Tùy chọn) Chuyển query string thành chữ thường
            });

            // Cấu hình cookie để chuyển hướng khi chưa đăng nhập
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login"; // Đảm bảo đường dẫn đăng nhập phù hợp
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            });
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

            app.UseEndpoints(endpoints =>
            {
                // Route ngắn cho tất cả action trong HomeController (Customer Area)
                endpoints.MapAreaControllerRoute(
                    name: "customer_pages",
                    areaName: "Customer",
                    pattern: "{action=Index}/{id?}",
                    defaults: new { controller = "Home" });

                // Route mặc định fallback cho các controller khác trong Customer Area
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