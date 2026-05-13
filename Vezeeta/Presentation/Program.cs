using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Services;
using Infranstructure.Persistence.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Presentation.Extensions;
using Infrastructure.Persistence;

namespace Presentation
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure services for the application.
            builder.Services.AddUserServices(builder.Configuration);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var context = services.GetRequiredService<ApplicationDbContext>();

                await DataSeeder.SeedAsync(context);
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

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
