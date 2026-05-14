using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Services;
using Infranstructure.Persistence.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Presentation.Extensions;

namespace Presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ==================== DATABASE & REPOSITORY/SERVICE CONFIGURATION ====================
            // ApplicationDbContext and IUnitOfWork are registered in AddUserServices
            builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
            builder.Services.AddScoped<IDoctorService, DoctorService>();




            // Configure services for the application.
            builder.Services.AddUserServices(builder.Configuration);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            // HTTP context accessor for fetching current user id in controllers
            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseSharedMiddleware();
            app.UseRouting();

            app.UseAuthorization();
            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Doctor}/{action=Index}/{id?}")
                .WithStaticAssets();


            app.Run();
        }
    }
}
