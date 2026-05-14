using Application.Interfaces.Repositories;
using Infranstructure.Persistence.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces.Services;
using Application.Services;
using Infrastructure.Services;
using Domain.Identity;
using Microsoft.AspNetCore.Identity;


namespace Presentation.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUserServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            
            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            services.AddScoped<IPatientProfileService, PatientProfileService>();

            return services;
        }
    }
}
