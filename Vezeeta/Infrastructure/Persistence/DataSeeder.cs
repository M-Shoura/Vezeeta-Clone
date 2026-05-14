using Domain.Entities;
using Domain.Identity;
using Infranstructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Infrastructure.Persistence
{
    public class DataSeeder
    {
        private sealed class ReviewSeedItem
        {
            public int Rating { get; set; }
            public string? Comment { get; set; }
            public int DaysAgo { get; set; }
        }

        public static async Task SeedAsync(ApplicationDbContext context)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            if (!context.Drugs.Any())
            {
                var drugsData = File.ReadAllText(
                    "../Infrastructure/Persistence/Seed/drugs_seed.json");

                var drugs = JsonSerializer.Deserialize<List<Drug>>(drugsData, options);

                if (drugs != null && drugs.Count > 0)
                {
                    foreach (var drug in drugs)
                    {
                        // IMPORTANT FIX
                        drug.PrescriptionItems = new HashSet<PrescriptionItem>();

                        // SAFETY CHECK
                        if (string.IsNullOrWhiteSpace(drug.Name))
                            continue;

                        context.Drugs.Add(drug);
                    }

                    await context.SaveChangesAsync();
                }
            }

            // Seed basic users (doctors + patients)
            if (!context.Users.Any())
            {
                var doctor1Id = Guid.NewGuid().ToString();
                var doctor2Id = Guid.NewGuid().ToString();
                var patient1Id = Guid.NewGuid().ToString();

                var users = new List<ApplicationUser>
                {
                    new ApplicationUser { Id = doctor1Id, UserName = "dr.john", NormalizedUserName = "DR.JOHN", Email = "dr.john@example.com", NormalizedEmail = "DR.JOHN@EXAMPLE.COM", FullName = "Dr. John Doe" },
                    new ApplicationUser { Id = doctor2Id, UserName = "dr.sara", NormalizedUserName = "DR.SARA", Email = "dr.sara@example.com", NormalizedEmail = "DR.SARA@EXAMPLE.COM", FullName = "Dr. Sara Lee" },
                    new ApplicationUser { Id = patient1Id, UserName = "patient.alex", NormalizedUserName = "PATIENT.ALEX", Email = "alex@example.com", NormalizedEmail = "ALEX@EXAMPLE.COM", FullName = "Alex Patient" }
                };

                context.Users.AddRange(users);
                await context.SaveChangesAsync();

                // Doctor profiles
                if (!context.DoctorProfiles.Any())
                {
                    var dp1 = new DoctorProfile
                    {
                        ApplicationUserId = doctor1Id,
                        Specialization = "Cardiology",
                        YearsOfExperience = 12,
                        Qualification = "MD",
                        IsAvailable = true,
                        LicenseNumber = "LIC-1001"
                    };

                    var dp2 = new DoctorProfile
                    {
                        ApplicationUserId = doctor2Id,
                        Specialization = "Dermatology",
                        YearsOfExperience = 8,
                        Qualification = "MD",
                        IsAvailable = true,
                        LicenseNumber = "LIC-1002"
                    };

                    context.DoctorProfiles.AddRange(dp1, dp2);
                }

                // Patient profiles
                if (!context.PatientProfiles.Any())
                {
                    var pp1 = new PatientProfile
                    {
                        ApplicationUserId = patient1Id,
                        DateOfBirth = DateTime.UtcNow.AddYears(-30),
                        Gender = Domain.Enums.Gender.Male,
                        EmergencyContactName = "Jane Doe",
                        EmergencyContactPhone = "555-0101"
                    };

                    context.PatientProfiles.Add(pp1);
                }

                await context.SaveChangesAsync();

                // Clinics
                if (!context.Clinics.Any())
                {
                    var clinic1 = new Clinic { Name = "Central Clinic", Address = "123 Main St", PhoneNumber = "555-1000", Email = "info@centralclinic.example" };
                    var clinic2 = new Clinic { Name = "Westside Clinic", Address = "45 West Ave", PhoneNumber = "555-2000", Email = "contact@westside.example" };
                    context.Clinics.AddRange(clinic1, clinic2);
                    await context.SaveChangesAsync();

                    // DoctorClinics
                    var doctorClinics = new List<DoctorClinic>
                    {
                        new DoctorClinic { DoctorId = doctor1Id, ClinicId = clinic1.Id, ConsultationFee = 150 },
                        new DoctorClinic { DoctorId = doctor2Id, ClinicId = clinic2.Id, ConsultationFee = 120 }
                    };

                    context.DoctorClinics.AddRange(doctorClinics);
                    await context.SaveChangesAsync();

                    // Doctor schedules (simple)
                    context.DoctorSchedules.Add(new DoctorSchedule { DoctorId = doctor1Id, ClinicId = clinic1.Id, Day = DayOfWeek.Monday, StartTime = new TimeSpan(9,0,0), EndTime = new TimeSpan(12,0,0) });
                    context.DoctorSchedules.Add(new DoctorSchedule { DoctorId = doctor2Id, ClinicId = clinic2.Id, Day = DayOfWeek.Wednesday, StartTime = new TimeSpan(13,0,0), EndTime = new TimeSpan(17,0,0) });
                    await context.SaveChangesAsync();

                    // Appointments
                    var appt1 = new Appointment
                    {
                        AppointmentDate = DateTime.UtcNow.Date.AddDays(2),
                        StartTime = new TimeSpan(9, 0, 0),
                        EndTime = new TimeSpan(9, 30, 0),
                        Status = Domain.Enums.AppointmentStatus.Confirmed,
                        DoctorId = doctor1Id,
                        PatientId = patient1Id,
                        ClinicId = clinic1.Id,
                        PaymentId = 0,
                        PrescriptionId = 0,
                        ReviewId = 0
                    };

                    context.Appointments.Add(appt1);
                    await context.SaveChangesAsync();

                    // Payment for appointment
                    var payment = new Payment { Amount = 150m, PaymentMethod = Domain.Enums.PaymentMethod.Visa, Status = Domain.Enums.PaymentStatus.Completed, PaidAt = DateTime.UtcNow, AppointmentId = appt1.Id };
                    context.Payments.Add(payment);
                    await context.SaveChangesAsync();

                    // Prescription and items
                    var prescription = new Prescription { AppointmentId = appt1.Id, PrescriptionDate = DateTime.UtcNow, Notes = "Take as prescribed" };
                    context.Prescriptions.Add(prescription);
                    await context.SaveChangesAsync();

                    // Attach a prescription item to an existing drug if available
                    var firstDrug = await context.Drugs.FirstOrDefaultAsync();
                    if (firstDrug != null)
                    {
                        var item = new PrescriptionItem { PrescriptionId = prescription.Id, DrugId = firstDrug.Id, Dosage = "1 pill", DurationInDays = 7, TimesPerDay = 2 };
                        context.PrescriptionItems.Add(item);
                        await context.SaveChangesAsync();
                    }

                    // Medical record
                    var mrec = new MedicalRecord { PatientId = patient1Id, Condition = "Hypertension", DiagnosedDate = DateTime.UtcNow.AddYears(-1), AppointmentId = appt1.Id };
                    context.MedicalRecords.Add(mrec);
                    await context.SaveChangesAsync();
                }
            }

            // Seed many reviews from JSON file.
            if (!context.Reviews.Any())
            {
                var doctors = await context.DoctorProfiles
                    .AsNoTracking()
                    .Select(d => d.ApplicationUserId)
                    .ToListAsync();

                var patients = await context.PatientProfiles
                    .AsNoTracking()
                    .Select(p => p.ApplicationUserId)
                    .ToListAsync();

                var clinics = await context.Clinics
                    .AsNoTracking()
                    .Select(c => c.Id)
                    .ToListAsync();

                // Ensure minimum references exist, even if users table is already populated.
                if (doctors.Count == 0 || patients.Count == 0 || clinics.Count == 0)
                {
                    var seedDoctorUser = await context.Users.FirstOrDefaultAsync(u => u.UserName == "seed.doctor");
                    if (seedDoctorUser == null)
                    {
                        seedDoctorUser = new ApplicationUser
                        {
                            Id = Guid.NewGuid().ToString(),
                            UserName = "seed.doctor",
                            NormalizedUserName = "SEED.DOCTOR",
                            Email = "seed.doctor@example.com",
                            NormalizedEmail = "SEED.DOCTOR@EXAMPLE.COM",
                            FullName = "Seed Doctor"
                        };
                        context.Users.Add(seedDoctorUser);
                        await context.SaveChangesAsync();
                    }

                    if (!await context.DoctorProfiles.AnyAsync(d => d.ApplicationUserId == seedDoctorUser.Id))
                    {
                        context.DoctorProfiles.Add(new DoctorProfile
                        {
                            ApplicationUserId = seedDoctorUser.Id,
                            Specialization = "General Medicine",
                            YearsOfExperience = 10,
                            Qualification = "MD",
                            IsAvailable = true,
                            LicenseNumber = $"SEED-{Guid.NewGuid():N}"[..13]
                        });
                        await context.SaveChangesAsync();
                    }

                    var seedPatientUser = await context.Users.FirstOrDefaultAsync(u => u.UserName == "seed.patient");
                    if (seedPatientUser == null)
                    {
                        seedPatientUser = new ApplicationUser
                        {
                            Id = Guid.NewGuid().ToString(),
                            UserName = "seed.patient",
                            NormalizedUserName = "SEED.PATIENT",
                            Email = "seed.patient@example.com",
                            NormalizedEmail = "SEED.PATIENT@EXAMPLE.COM",
                            FullName = "Seed Patient"
                        };
                        context.Users.Add(seedPatientUser);
                        await context.SaveChangesAsync();
                    }

                    if (!await context.PatientProfiles.AnyAsync(p => p.ApplicationUserId == seedPatientUser.Id))
                    {
                        context.PatientProfiles.Add(new PatientProfile
                        {
                            ApplicationUserId = seedPatientUser.Id,
                            DateOfBirth = DateTime.UtcNow.AddYears(-28),
                            Gender = Domain.Enums.Gender.Male,
                            EmergencyContactName = "Seed Contact",
                            EmergencyContactPhone = "555-9000"
                        });
                        await context.SaveChangesAsync();
                    }

                    if (!await context.Clinics.AnyAsync())
                    {
                        context.Clinics.Add(new Clinic
                        {
                            Name = "Seed Clinic",
                            Address = "100 Seed Street",
                            PhoneNumber = "555-9001",
                            Email = "seed.clinic@example.com"
                        });
                        await context.SaveChangesAsync();
                    }

                    doctors = await context.DoctorProfiles
                        .AsNoTracking()
                        .Select(d => d.ApplicationUserId)
                        .ToListAsync();

                    patients = await context.PatientProfiles
                        .AsNoTracking()
                        .Select(p => p.ApplicationUserId)
                        .ToListAsync();

                    clinics = await context.Clinics
                        .AsNoTracking()
                        .Select(c => c.Id)
                        .ToListAsync();
                }

                if (doctors.Count > 0 && patients.Count > 0 && clinics.Count > 0)
                {
                    var reviewsData = File.ReadAllText("../Infrastructure/Persistence/Seed/reviews_seed.json");
                    var reviewSeeds = JsonSerializer.Deserialize<List<ReviewSeedItem>>(reviewsData, options) ?? new List<ReviewSeedItem>();

                    if (reviewSeeds.Count > 0)
                    {
                        var seedAppointments = new List<Appointment>();

                        for (var i = 0; i < reviewSeeds.Count; i++)
                        {
                            var seed = reviewSeeds[i];
                            var safeRating = Math.Clamp(seed.Rating, 1, 5);
                            var daysAgo = seed.DaysAgo > 0 ? seed.DaysAgo : (i + 1);

                            var appointmentDate = DateTime.UtcNow.Date.AddDays(-daysAgo);
                            var startHour = 9 + (i % 7);

                            seedAppointments.Add(new Appointment
                            {
                                AppointmentDate = appointmentDate,
                                StartTime = new TimeSpan(startHour, 0, 0),
                                EndTime = new TimeSpan(startHour, 30, 0),
                                Status = Domain.Enums.AppointmentStatus.Completed,
                                DoctorId = doctors[i % doctors.Count],
                                PatientId = patients[i % patients.Count],
                                ClinicId = clinics[i % clinics.Count],
                                PaymentId = 0,
                                PrescriptionId = 0,
                                ReviewId = 0,
                                Notes = "Auto-seeded appointment for review data"
                            });

                            reviewSeeds[i].Rating = safeRating;
                        }

                        context.Appointments.AddRange(seedAppointments);
                        await context.SaveChangesAsync();

                        var reviewEntities = new List<Review>();
                        for (var i = 0; i < reviewSeeds.Count; i++)
                        {
                            var seed = reviewSeeds[i];
                            var appointment = seedAppointments[i];

                            reviewEntities.Add(new Review
                            {
                                Rating = seed.Rating,
                                Comment = string.IsNullOrWhiteSpace(seed.Comment) ? "No comment" : seed.Comment,
                                AppointmentId = appointment.Id,
                                DoctorId = appointment.DoctorId!,
                                PatientId = appointment.PatientId!
                            });
                        }

                        context.Reviews.AddRange(reviewEntities);
                        await context.SaveChangesAsync();
                    }
                }
            }
        }
    }
}