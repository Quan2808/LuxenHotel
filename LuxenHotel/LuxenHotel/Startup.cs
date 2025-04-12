﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Identity;
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
                // Route mặc định cho Customer Area (không chứa "Customer")
                endpoints.MapAreaControllerRoute(
                    name: "customer_default",
                    areaName: "Customer",
                    pattern: "",
                    defaults: new { controller = "Home", action = "Index" });

                // Route cho các controller khác trong Customer Area
                endpoints.MapAreaControllerRoute(
                    name: "customer_area",
                    areaName: "Customer",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                // Route cho Admin Area
                endpoints.MapAreaControllerRoute(
                    name: "admin_area",
                    areaName: "Admin",
                    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}");

                // Route cho Staff Area
                endpoints.MapAreaControllerRoute(
                    name: "staff_area",
                    areaName: "Staff",
                    pattern: "Staff/{controller=Task}/{action=Index}/{id?}");

                // Route cho Identity Area (nếu có)
                endpoints.MapAreaControllerRoute(
                    name: "identity_area",
                    areaName: "Identity",
                    pattern: "Identity/{controller=Account}/{action=Login}/{id?}");
            });
        }
    }
}