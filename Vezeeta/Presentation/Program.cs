using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Auth;
using Application.Services;
using Domain.Identity;
using Infranstructure.Persistence.Data;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Presentation.Extensions;
using Vezeeta.Domain.Interfaces.Repositories;
using Vezeeta.Domain.Interfaces.Services;
using Vezeeta.Application.Services;
using Vezeeta.Infrastructure.Repositories;

namespace Presentation
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            // ==================== Identity CONFIGURATION ====================
            builder.Services.AddAuthentication().AddGoogle(options => {
                options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
                options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
            });
            builder.Services.AddIdentity<ApplicationUser,ApplicationRole>(o=>
            {
                o.Password.RequiredUniqueChars = 1;
                o.Password.RequireNonAlphanumeric = true;
                o.Password.RequireDigit = true;
                o.Password.RequiredLength = 8;
                o.Password.RequireUppercase = true;
            }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IPasswordService, PasswordService>();
            // ==================== DATABASE CONFIGURATION ====================
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
            builder.Services.AddScoped<IDoctorService, DoctorService>();




            // Configure services for the application.
            builder.Services.AddUserServices(builder.Configuration);
            
            // Add services to the container.
            
            builder.Services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();

                options.Filters.Add(new AuthorizeFilter(policy));
            });


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

            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseSharedMiddleware();
            app.UseRouting();

            app.UseAuthentication();
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
