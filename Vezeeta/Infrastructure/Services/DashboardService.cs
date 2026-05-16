using Application.DTOs.Dashboards;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Identity;
using Infranstructure.Persistence.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<AdminDashboardViewModel> GetAdminDashboardAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var timeOfDay = now.TimeOfDay;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var chartStart = monthStart.AddMonths(-5);

        var appointmentBase = _context.Appointments.AsNoTracking();
        var paymentBase = _context.Payments.AsNoTracking();

        var recentAppointments = await appointmentBase
            .Include(a => a.Patient).ThenInclude(p => p.ApplicationUser)
            .Include(a => a.Doctor).ThenInclude(d => d.ApplicationUser)
            .Include(a => a.Clinic)
            .OrderByDescending(a => a.AppointmentDate)
            .ThenByDescending(a => a.StartTime)
            .Take(8)
            .Select(a => new RecentAppointmentDashboardDto
            {
                Id = a.Id,
                ReviewId = a.ReviewId,
                PatientName = a.Patient.ApplicationUser.FullName,
                DoctorName = a.Doctor.ApplicationUser.FullName,
                ClinicName = a.Clinic.Name,
                AppointmentDate = a.AppointmentDate,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = a.Status,
                HasPrescription = a.Prescription != null,
                PrescriptionId = a.Prescription != null ? a.Prescription.Id : null
            })
            .ToListAsync(cancellationToken);

        var monthlyAppointmentsRaw = await appointmentBase
            .Where(a => a.AppointmentDate >= chartStart)
            .GroupBy(a => new { a.AppointmentDate.Year, a.AppointmentDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToListAsync(cancellationToken);

        var monthlyRevenueRaw = await paymentBase
            .Where(p => p.Status == PaymentStatus.Completed && p.PaidAt != null && p.PaidAt >= chartStart)
            .GroupBy(p => new { p.PaidAt!.Value.Year, p.PaidAt.Value.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Amount = g.Sum(p => p.Amount) })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToListAsync(cancellationToken);

        var topDoctors = await appointmentBase
            .Include(a => a.Doctor).ThenInclude(d => d.ApplicationUser)
            .Include(a => a.Payment)
            .GroupBy(a => new
            {
                DoctorId = a.DoctorId ?? string.Empty,
                DoctorName = a.Doctor.ApplicationUser.FullName,
                a.Doctor.Specialization
            })
            .Select(g => new TopDoctorDashboardDto
            {
                DoctorId = g.Key.DoctorId,
                DoctorName = g.Key.DoctorName,
                Specialization = g.Key.Specialization,
                AppointmentCount = g.Count(),
                Revenue = g.Where(a => a.Payment != null && a.Payment.Status == PaymentStatus.Completed).Sum(a => a.Payment!.Amount)
            })
            .OrderByDescending(d => d.AppointmentCount)
            .Take(5)
            .ToListAsync(cancellationToken);

        var users = await _userManager.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAt)
            .Take(8)
            .ToListAsync(cancellationToken);

        var recentUsers = new List<RecentUserDashboardDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            recentUsers.Add(new RecentUserDashboardDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Role = roles.FirstOrDefault() ?? "User",
                CreatedAt = user.CreatedAt
            });
        }

        return new AdminDashboardViewModel
        {
            DoctorsCount = await _context.DoctorProfiles.CountAsync(cancellationToken),
            PatientsCount = await _context.PatientProfiles.CountAsync(cancellationToken),
            ClinicsCount = await _context.Clinics.CountAsync(cancellationToken),
            TotalAppointments = await appointmentBase.CountAsync(cancellationToken),
            TodayAppointments = await appointmentBase.CountAsync(a => a.AppointmentDate.Date == today, cancellationToken),
            PendingAppointments = await appointmentBase.CountAsync(a => a.Status == AppointmentStatus.Pending, cancellationToken),
            CompletedAppointments = await appointmentBase.CountAsync(a => a.Status == AppointmentStatus.Completed, cancellationToken),
            TotalRevenue = await paymentBase.Where(p => p.Status == PaymentStatus.Completed).SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0,
            MonthlyRevenue = await paymentBase.Where(p => p.Status == PaymentStatus.Completed && p.PaidAt != null && p.PaidAt >= monthStart).SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0,
            RecentAppointments = recentAppointments,
            RecentUsers = recentUsers,
            MonthlyAppointments = FillMonthSeries(chartStart, monthlyAppointmentsRaw.Select(x => (x.Year, x.Month, x.Count, Amount: 0m))),
            MonthlyRevenuePoints = FillMonthSeries(chartStart, monthlyRevenueRaw.Select(x => (x.Year, x.Month, Count: 0, x.Amount))),
            TopDoctors = topDoctors
        };
    }

    public async Task<DoctorDashboardViewModel?> GetDoctorDashboardAsync(string doctorUserId, CancellationToken cancellationToken = default)
    {
        var doctor = await _context.DoctorProfiles
            .AsNoTracking()
            .Include(d => d.ApplicationUser)
            .FirstOrDefaultAsync(d => d.ApplicationUserId == doctorUserId, cancellationToken);

        if (doctor == null)
            return null;

        var today = DateTime.Today;
        var appointmentsQuery = _context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorUserId)
            .Include(a => a.Patient).ThenInclude(p => p.ApplicationUser)
            .Include(a => a.Doctor).ThenInclude(d => d.ApplicationUser)
            .Include(a => a.Clinic)
            .Include(a => a.Payment);

        var todaysAppointments = await appointmentsQuery
            .Where(a => a.AppointmentDate.Date == today)
            .OrderBy(a => a.StartTime)
            .Select(a => new RecentAppointmentDashboardDto
            {
                Id = a.Id,
                ReviewId = a.ReviewId,
                PatientName = a.Patient.ApplicationUser.FullName,
                DoctorName = a.Doctor.ApplicationUser.FullName,
                ClinicName = a.Clinic.Name,
                AppointmentDate = a.AppointmentDate,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = a.Status,
                HasPrescription = a.Prescription != null,
                PrescriptionId = a.Prescription != null ? a.Prescription.Id : null
            })
            .ToListAsync(cancellationToken);

        var upcomingAppointments = await appointmentsQuery
            .Where(a => a.AppointmentDate.Date >= today)
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.StartTime)
            .Take(8)
            .Select(a => new RecentAppointmentDashboardDto
            {
                Id = a.Id,
                ReviewId = a.ReviewId,
                PatientName = a.Patient.ApplicationUser.FullName,
                DoctorName = a.Doctor.ApplicationUser.FullName,
                ClinicName = a.Clinic.Name,
                AppointmentDate = a.AppointmentDate,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = a.Status,
                HasPrescription = a.Prescription != null,
                PrescriptionId = a.Prescription != null ? a.Prescription.Id : null
            })
            .ToListAsync(cancellationToken);

        var schedules = await _context.DoctorSchedules
            .AsNoTracking()
            .Where(s => s.DoctorId == doctorUserId && s.IsActive)
            .OrderBy(s => s.Day)
            .ThenBy(s => s.StartTime)
            .Select(s => new DoctorScheduleDashboardDto
            {
                Id = s.Id,
                Day = s.Day,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                SlotDurationMinutes = s.SlotDurationMinutes
            })
            .ToListAsync(cancellationToken);

        var clinics = await _context.DoctorClinics
            .AsNoTracking()
            .Where(c => c.DoctorId == doctorUserId)
            .Include(c => c.Clinic)
            .Select(c => new DoctorClinicDashboardDto
            {
                ClinicId = c.ClinicId,
                ClinicName = c.Clinic.Name,
                ConsultationFee = c.ConsultationFee
            })
            .ToListAsync(cancellationToken);

        return new DoctorDashboardViewModel
        {
            DoctorId = doctorUserId,
            FullName = doctor.ApplicationUser.FullName,
            ProfilePicture = doctor.ApplicationUser.ProfilePicture,
            Specialization = doctor.Specialization,
            YearsOfExperience = doctor.YearsOfExperience,
            IsAvailable = doctor.IsAvailable,
            TodaysAppointmentsCount = todaysAppointments.Count,
            UpcomingAppointmentsCount = await appointmentsQuery.CountAsync(a => a.AppointmentDate.Date >= today, cancellationToken),
            TotalPatients = await appointmentsQuery.Select(a => a.PatientId).Distinct().CountAsync(cancellationToken),
            PendingRequests = await appointmentsQuery.CountAsync(a => a.Status == AppointmentStatus.Pending, cancellationToken),
            CompletedSessions = await appointmentsQuery.CountAsync(a => a.Status == AppointmentStatus.Completed, cancellationToken),
            Earnings = await appointmentsQuery.Where(a => a.Payment != null && a.Payment.Status == PaymentStatus.Completed).SumAsync(a => (decimal?)a.Payment!.Amount, cancellationToken) ?? 0,
            TodaysAppointments = todaysAppointments,
            UpcomingAppointments = upcomingAppointments,
            Schedules = schedules,
            Clinics = clinics
        };
    }

    public async Task<PatientDashboardViewModel?> GetPatientDashboardAsync(string patientUserId, CancellationToken cancellationToken = default)
    {
        var patient = await _context.PatientProfiles
            .AsNoTracking()
            .Include(p => p.ApplicationUser)
            .FirstOrDefaultAsync(p => p.ApplicationUserId == patientUserId, cancellationToken);

        if (patient == null)
            return null;

        var today = DateTime.Today;
        var now = DateTime.Now;
        var timeOfDay = now.TimeOfDay;
        var appointmentsQuery = _context.Appointments
            .AsNoTracking()
            .Where(a => a.PatientId == patientUserId)
            .Include(a => a.Patient).ThenInclude(p => p.ApplicationUser)
            .Include(a => a.Doctor).ThenInclude(d => d.ApplicationUser)
            .Include(a => a.Clinic);

        var upcoming = await appointmentsQuery
            .Where(a => a.AppointmentDate.Date >= today)
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.StartTime)
            //.Take(6)
            .Select(a => new RecentAppointmentDashboardDto
            {
                Id = a.Id,
                PatientName = a.Patient.ApplicationUser.FullName,
                DoctorName = a.Doctor.ApplicationUser.FullName,
                ClinicName = a.Clinic.Name,
                AppointmentDate = a.AppointmentDate,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = a.Status,
                HasPrescription = a.Prescription != null,
                PrescriptionId = a.Prescription != null ? a.Prescription.Id : null
            })
            .ToListAsync(cancellationToken);

        var history = await appointmentsQuery
            .Where(a => a.AppointmentDate.Date < today || (a.AppointmentDate.Date == today && a.EndTime <= timeOfDay))
            .OrderByDescending(a => a.AppointmentDate)
            .ThenByDescending(a => a.EndTime)
            .Take(6)
            .Select(a => new RecentAppointmentDashboardDto
            {
                Id = a.Id,
                ReviewId = a.ReviewId,
                PatientName = a.Patient.ApplicationUser.FullName,
                DoctorName = a.Doctor.ApplicationUser.FullName,
                ClinicName = a.Clinic.Name,
                AppointmentDate = a.AppointmentDate,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = a.Status,
                HasPrescription = a.Prescription != null,
                PrescriptionId = a.Prescription != null ? a.Prescription.Id : null
            })
            .ToListAsync(cancellationToken);

        var prescriptions = await _context.Prescriptions
            .AsNoTracking()
            .Where(p => p.Appointment.PatientId == patientUserId)
            .Include(p => p.Appointment).ThenInclude(a => a.Doctor).ThenInclude(d => d.ApplicationUser)
            .Include(p => p.PrescriptionItems)
            .OrderByDescending(p => p.PrescriptionDate)
            .Take(5)
            .Select(p => new PatientPrescriptionDashboardDto
            {
                Id = p.Id,
                PrescriptionDate = p.PrescriptionDate,
                DoctorName = p.Appointment.Doctor.ApplicationUser.FullName,
                ItemsCount = p.PrescriptionItems.Count,
                Notes = p.Notes
            })
            .ToListAsync(cancellationToken);

        var medicalSummaries = await _context.MedicalRecords
            .AsNoTracking()
            .Where(r => r.PatientId == patientUserId)
            .OrderByDescending(r => r.DiagnosedDate)
            .Take(5)
            .Select(r => new PatientMedicalSummaryDto
            {
                Id = r.Id,
                Condition = r.Condition,
                DiagnosedDate = r.DiagnosedDate,
                IsActive = r.IsActive
            })
            .ToListAsync(cancellationToken);

        return new PatientDashboardViewModel
        {
            PatientId = patientUserId,
            FullName = patient.ApplicationUser.FullName,
            Email = patient.ApplicationUser.Email ?? string.Empty,
            PhoneNumber = patient.ApplicationUser.PhoneNumber,
            ProfilePicture = patient.ApplicationUser.ProfilePicture,
            DateOfBirth = patient.DateOfBirth,
            BloodType = patient.BloodType,
            ProfileCompletion = CalculateProfileCompletion(patient),
            UpcomingAppointmentsCount = await appointmentsQuery.CountAsync(a => a.AppointmentDate.Date >= today, cancellationToken),
            AppointmentHistoryCount = await appointmentsQuery.CountAsync(a => a.AppointmentDate.Date < today || (a.AppointmentDate.Date == today && a.EndTime <= timeOfDay), cancellationToken),
            PrescriptionCount = await _context.Prescriptions.CountAsync(p => p.Appointment.PatientId == patientUserId, cancellationToken),
            MedicalRecordCount = await _context.MedicalRecords.CountAsync(r => r.PatientId == patientUserId, cancellationToken),
            FavoriteDoctorsCount = 0,
            UpcomingAppointments = upcoming,
            AppointmentHistory = history,
            Prescriptions = prescriptions,
            MedicalSummaries = medicalSummaries
        };
    }

    private static int CalculateProfileCompletion(PatientProfile patient)
    {
        var completed = 0;
        var total = 7;

        if (!string.IsNullOrWhiteSpace(patient.ApplicationUser.FullName)) completed++;
        if (!string.IsNullOrWhiteSpace(patient.ApplicationUser.Email)) completed++;
        if (!string.IsNullOrWhiteSpace(patient.ApplicationUser.PhoneNumber)) completed++;
        if (patient.DateOfBirth != default) completed++;
        if (!string.IsNullOrWhiteSpace(patient.BloodType)) completed++;
        if (!string.IsNullOrWhiteSpace(patient.EmergencyContactName)) completed++;
        if (!string.IsNullOrWhiteSpace(patient.EmergencyContactPhone)) completed++;

        return (int)Math.Round(completed * 100m / total);
    }

    private static IReadOnlyList<ChartPointDto> FillMonthSeries(
        DateTime start,
        IEnumerable<(int Year, int Month, int Count, decimal Amount)> source)
    {
        var lookup = source.ToDictionary(x => (x.Year, x.Month));
        var points = new List<ChartPointDto>();

        for (var i = 0; i < 6; i++)
        {
            var month = start.AddMonths(i);
            lookup.TryGetValue((month.Year, month.Month), out var value);
            points.Add(new ChartPointDto
            {
                Label = month.ToString("MMM yyyy"),
                Count = value.Count,
                Amount = value.Amount
            });
        }

        return points;
    }
}
