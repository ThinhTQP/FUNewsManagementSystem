
using FUNews.BLL.Services;
using FUNews.BLL.UnitOfWorks;
using FUNews.DAL;
using FUNews.DAL.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FUNewsManagementMVC
{
    public static class ServiceCollectionExtension
    {
        public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<FUNewsManagementContext>
            (
              option => option.UseSqlServer(configuration["ConnectionStrings:DefaultConnection"])
            );
        }
        public static void AddRepositoryUOW(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));
            services.AddScoped<CategoryService>();
            services.AddScoped<TagService>();
            services.AddScoped<NewsArticleService>();
            services.AddScoped<SystemAccountService>();
        }
        public static void AddAuthen(this IServiceCollection services)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
     .AddCookie(options =>
     {
         options.LoginPath = "/SystemAccounts/Login";
         options.AccessDeniedPath = "/SystemAccounts/AccessDenied";
     });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));

                options.AddPolicy("StaffOnly", policy => policy.RequireRole("Staff"));
                options.AddPolicy("LecturerOnly", policy => policy.RequireRole("Lecturer"));
            });

        }
    }
}
