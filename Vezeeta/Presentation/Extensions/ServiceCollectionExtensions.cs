using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Auth;
using Application.Mappings;
using Application.Services;
using Domain.Identity;
using Infranstructure.Persistence.Data;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.Services.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace Presentation.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUserServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));



            // ==================== Identity CONFIGURATION ====================
            services.AddAuthentication().AddGoogle(options => {
                options.ClientId = configuration["Authentication:Google:ClientId"]!;
                options.ClientSecret = configuration["Authentication:Google:ClientSecret"]!;
            });
            services.AddIdentity<ApplicationUser, ApplicationRole>(o =>
            {
                o.Password.RequiredUniqueChars = 1;
                o.Password.RequireNonAlphanumeric = true;
                o.Password.RequireDigit = true;
                o.Password.RequiredLength = 8;
                o.Password.RequireUppercase = true;
            }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

            // ==================== DATABASE CONFIGURATION ====================


            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IDoctorRepository, DoctorRepository>();
            services.AddScoped<IDoctorService, DoctorService>();



            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IPasswordService, PasswordService>();

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IDoctorRepository, DoctorRepository>();
            services.AddScoped<IDoctorService, DoctorService>();

            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            services.AddScoped<IPatientProfileService, PatientProfileService>();
            services.AddScoped<IPatientRepository, PatientRepository>();

            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IReviewService,  ReviewService>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IDashboardService, DashboardService>();

            services.AddScoped<IDrugsService, DrugService>();
            services.AddScoped<IPrescriptionService, PrescriptionService>();
            services.AddScoped<IPrescriptionItemService, PrescriptionItemService>();
            services.AddAutoMapper(typeof(MappingProfile).Assembly);
            services.AddScoped<IMedicalRecordService, MedicalRecordService>();
            return services;
        }
    }
}
