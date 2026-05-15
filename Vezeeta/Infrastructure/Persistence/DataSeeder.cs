using Domain.Entities;
using Domain.Enums;
using Domain.Identity;
using Infranstructure.Persistence.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Infrastructure.Persistence
{
    public class DataSeeder
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static async Task SeedAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            await context.Database.MigrateAsync();

            await SeedRolesAsync(roleManager);
            await SeedUsersAsync(userManager);
            await SeedClinicsAsync(context);
            await SeedDoctorProfilesAsync(context);
            await SeedPatientProfilesAsync(context);
            await SeedDoctorClinicsAsync(context);
            await SeedDoctorSchedulesAsync(context);
            await SeedAppointmentsAsync(context);
            await SeedDrugsAsync(context);
        }

        private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
        {
            var roles = new[]
            {
                UserRole.Admin.ToString(),
                UserRole.Doctor.ToString(),
                UserRole.Patient.ToString()
            };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
                }
            }
        }

        private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
        {
            var users = LoadSeed<List<UserSeed>>("users_seed.json") ?? new List<UserSeed>();

            foreach (var seed in users)
            {
                if (await userManager.FindByEmailAsync(seed.Email) is not null)
                {
                    continue;
                }

                var user = new ApplicationUser
                {
                    Id = seed.Id,
                    UserName = seed.Email,
                    Email = seed.Email,
                    FullName = seed.FullName,
                    Address = seed.Address,
                    PhoneNumber = seed.PhoneNumber,
                    ProfilePicture = seed.ProfilePicture,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(user, seed.Password);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to create seed user '{seed.Email}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }

                if (!string.IsNullOrWhiteSpace(seed.Role))
                {
                    await userManager.AddToRoleAsync(user, seed.Role);
                }
            }
        }

        private static async Task SeedClinicsAsync(ApplicationDbContext context)
        {
            var clinics = LoadSeed<List<ClinicSeed>>("clinics_seed.json") ?? new List<ClinicSeed>();

            foreach (var seed in clinics)
            {
                if (await context.Clinics.AnyAsync(c => c.Name == seed.Name))
                {
                    continue;
                }

                context.Clinics.Add(new Clinic
                {
                    Name = seed.Name,
                    Address = seed.Address,
                    PhoneNumber = seed.PhoneNumber,
                    Description = seed.Description,
                    Email = seed.Email,
                    IsActive = seed.IsActive,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedDoctorProfilesAsync(ApplicationDbContext context)
        {
            var doctors = LoadSeed<List<DoctorSeed>>("doctors_seed.json") ?? new List<DoctorSeed>();

            foreach (var seed in doctors)
            {
                if (await context.DoctorProfiles.AnyAsync(d => d.ApplicationUserId == seed.ApplicationUserId))
                {
                    continue;
                }

                context.DoctorProfiles.Add(new DoctorProfile
                {
                    ApplicationUserId = seed.ApplicationUserId,
                    Specialization = seed.Specialization,
                    YearsOfExperience = seed.YearsOfExperience,
                    Bio = seed.Bio,
                    Qualification = seed.Qualification,
                    IsAvailable = seed.IsAvailable,
                    LicenseNumber = seed.LicenseNumber
                });
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedPatientProfilesAsync(ApplicationDbContext context)
        {
            var patients = LoadSeed<List<PatientSeed>>("patients_seed.json") ?? new List<PatientSeed>();

            foreach (var seed in patients)
            {
                if (await context.PatientProfiles.AnyAsync(p => p.ApplicationUserId == seed.ApplicationUserId))
                {
                    continue;
                }

                context.PatientProfiles.Add(new PatientProfile
                {
                    ApplicationUserId = seed.ApplicationUserId,
                    DateOfBirth = seed.DateOfBirth,
                    Gender = seed.Gender,
                    BloodType = seed.BloodType,
                    EmergencyContactName = seed.EmergencyContactName,
                    EmergencyContactPhone = seed.EmergencyContactPhone
                });
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedDoctorClinicsAsync(ApplicationDbContext context)
        {
            var doctorClinics = LoadSeed<List<DoctorClinicSeed>>("doctor_clinics_seed.json") ?? new List<DoctorClinicSeed>();

            foreach (var seed in doctorClinics)
            {
                var clinic = await context.Clinics.FirstOrDefaultAsync(c => c.Name == seed.ClinicName);
                if (clinic is null)
                {
                    continue;
                }

                if (await context.DoctorClinics.AnyAsync(dc => dc.DoctorId == seed.DoctorUserId && dc.ClinicId == clinic.Id))
                {
                    continue;
                }

                context.DoctorClinics.Add(new DoctorClinic
                {
                    DoctorId = seed.DoctorUserId,
                    ClinicId = clinic.Id,
                    ConsultationFee = seed.ConsultationFee,
                    IsAvailable = seed.IsAvailable,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedDoctorSchedulesAsync(ApplicationDbContext context)
        {
            var schedules = LoadSeed<List<DoctorScheduleSeed>>("doctor_schedules_seed.json") ?? new List<DoctorScheduleSeed>();

            foreach (var seed in schedules)
            {
                var clinic = await context.Clinics.FirstOrDefaultAsync(c => c.Name == seed.ClinicName);
                if (clinic is null)
                {
                    continue;
                }

                var exists = await context.DoctorSchedules.AnyAsync(ds =>
                    ds.DoctorId == seed.DoctorUserId &&
                    ds.ClinicId == clinic.Id &&
                    ds.Day == seed.Day &&
                    ds.StartTime == seed.StartTime &&
                    ds.EndTime == seed.EndTime);

                if (exists)
                {
                    continue;
                }

                context.DoctorSchedules.Add(new DoctorSchedule
                {
                    DoctorId = seed.DoctorUserId,
                    ClinicId = clinic.Id,
                    Day = seed.Day,
                    StartTime = seed.StartTime,
                    EndTime = seed.EndTime,
                    SlotDurationMinutes = seed.SlotDurationMinutes,
                    IsActive = seed.IsActive,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedAppointmentsAsync(ApplicationDbContext context)
        {
            var appointments = LoadSeed<List<AppointmentSeed>>("appointments_seed.json") ?? new List<AppointmentSeed>();

            foreach (var seed in appointments)
            {
                var clinic = await context.Clinics.FirstOrDefaultAsync(c => c.Name == seed.ClinicName);
                if (clinic is null)
                {
                    continue;
                }

                var exists = await context.Appointments.AnyAsync(a =>
                    a.DoctorId == seed.DoctorUserId &&
                    a.PatientId == seed.PatientUserId &&
                    a.ClinicId == clinic.Id &&
                    a.AppointmentDate == seed.AppointmentDate &&
                    a.StartTime == seed.StartTime &&
                    a.EndTime == seed.EndTime);

                if (exists)
                {
                    continue;
                }

                context.Appointments.Add(new Appointment
                {
                    AppointmentDate = seed.AppointmentDate,
                    StartTime = seed.StartTime,
                    EndTime = seed.EndTime,
                    Status = seed.Status,
                    Notes = seed.Notes,
                    DoctorId = seed.DoctorUserId,
                    PatientId = seed.PatientUserId,
                    ClinicId = clinic.Id,
                    PaymentId = 0,
                    PrescriptionId = 0,
                    ReviewId = 0,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedDrugsAsync(ApplicationDbContext context)
        {
            if (await context.Drugs.AnyAsync())
            {
                return;
            }

            var drugs = LoadSeed<List<Drug>>("drugs_seed.json") ?? new List<Drug>();

            foreach (var drug in drugs)
            {
                drug.PrescriptionItems = new HashSet<PrescriptionItem>();

                if (string.IsNullOrWhiteSpace(drug.Name))
                {
                    continue;
                }

                context.Drugs.Add(drug);
            }

            await context.SaveChangesAsync();
        }

        private static T? LoadSeed<T>(string fileName)
        {
            var seedPath = ResolveSeedPath(fileName);
            var json = File.ReadAllText(seedPath);
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }

        private static string ResolveSeedPath(string fileName)
        {
            var candidatePaths = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "Seed", fileName),
                Path.Combine(Directory.GetCurrentDirectory(), "Infrastructure", "Persistence", "Seed", fileName),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "Infrastructure", "Persistence", "Seed", fileName),
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Infrastructure", "Persistence", "Seed", fileName)
            };

            foreach (var candidate in candidatePaths)
            {
                var fullPath = Path.GetFullPath(candidate);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            throw new FileNotFoundException($"Seed file '{fileName}' was not found.");
        }

        private sealed record UserSeed(
            string Id,
            string Email,
            string Password,
            string FullName,
            string Role,
            string? Address,
            string? PhoneNumber,
            string? ProfilePicture);

        private sealed record ClinicSeed(
            string Name,
            string Address,
            string PhoneNumber,
            string? Description,
            string? Email,
            bool IsActive);

        private sealed record DoctorSeed(
            string ApplicationUserId,
            string Specialization,
            int YearsOfExperience,
            string Qualification,
            bool IsAvailable,
            string? Bio,
            string? LicenseNumber);

        private sealed record DoctorClinicSeed(
            string DoctorUserId,
            string ClinicName,
            decimal ConsultationFee,
            bool IsAvailable);

        private sealed record DoctorScheduleSeed(
            string DoctorUserId,
            string ClinicName,
            DayOfWeek Day,
            TimeSpan StartTime,
            TimeSpan EndTime,
            int SlotDurationMinutes,
            bool IsActive);

        private sealed record PatientSeed(
            string ApplicationUserId,
            DateTime DateOfBirth,
            Gender Gender,
            string? BloodType,
            string EmergencyContactName,
            string EmergencyContactPhone);

        private sealed record AppointmentSeed(
            string DoctorUserId,
            string PatientUserId,
            string ClinicName,
            DateTime AppointmentDate,
            TimeSpan StartTime,
            TimeSpan EndTime,
            AppointmentStatus Status,
            string? Notes);
    }
}