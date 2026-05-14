using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Infranstructure.Persistence.Data;
using Infrastructure.Repositories;
using Infrastructure.Services;
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

            services.AddScoped<IDoctorRepository, DoctorRepository>();
            services.AddScoped<IDoctorService, DoctorService>();

            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            services.AddScoped<IPatientProfileService, PatientProfileService>();

            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IReviewService,  ReviewService>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IPaymentService, PaymentService>();
            return services;
        }
    }
}
