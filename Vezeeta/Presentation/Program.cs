using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Auth;
using Application.Services;
using Domain.Identity;
using Infranstructure.Persistence.Data;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Presentation.Extensions;
using Presentation.Hubs; // ◄ 1. ADD THIS to import the folder where you saved your Hub!

namespace Presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure services for the application.
            builder.Services.AddUserServices(builder.Configuration);

            // 2. CLEANUP: We configure the 10MB limit globally here
            builder.Services.AddSignalR(options =>
            {
                // Extends the window allowed for a background process loop execution before dropping a socket connection
                options.ClientTimeoutInterval = TimeSpan.FromMinutes(2);
                options.KeepAliveInterval = TimeSpan.FromSeconds(30);
            });
            builder.Services.Configure<HubOptions>(options =>
            {
                options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10MB
            });

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.LoginPath = "/Account/Login";
            });

            builder.Services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();

                options.Filters.Add(new AuthorizeFilter(policy));
            });
            // Register the HttpClient context for your AI interactions
            builder.Services.AddHttpClient<IMedicalAiService, MedicalAiService>();
            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<ApplicationDbContext>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

                DataSeeder.SeedAsync(context, userManager, roleManager)
                    .GetAwaiter()
                    .GetResult();
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseSharedMiddleware();

            // IMPORTANT: Pipeline Order Matters!
            app.UseRouting();

            app.UseAuthentication();
            // Auth must be active BEFORE checking who is allowed on the SignalR Hub line
            app.UseAuthorization();

            // 3. MAP THE HUB HERE (Right after Authorization)
            app.MapHub<ConsultationHub>("/consultationHub");

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}