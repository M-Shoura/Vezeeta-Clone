using Domain.Enums;

namespace Application.DTOs.Dashboards;

public sealed class DashboardMetricDto
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string? DisplayValue { get; set; }
    public string? Hint { get; set; }
}

public sealed class RecentAppointmentDashboardDto
{
    public int Id { get; set; }
    public int ReviewId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string ClinicName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public AppointmentStatus Status { get; set; }
    public bool HasPrescription { get; set; }
    public int? PrescriptionId { get; set; }
}

public sealed class RecentUserDashboardDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public sealed class ChartPointDto
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Amount { get; set; }
}

public sealed class TopDoctorDashboardDto
{
    public string DoctorId { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public int AppointmentCount { get; set; }
    public decimal Revenue { get; set; }
}

public sealed class AdminDashboardViewModel
{
    public int DoctorsCount { get; set; }
    public int PatientsCount { get; set; }
    public int ClinicsCount { get; set; }
    public int TotalAppointments { get; set; }
    public int TodayAppointments { get; set; }
    public int PendingAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public IReadOnlyList<RecentAppointmentDashboardDto> RecentAppointments { get; set; } = [];
    public IReadOnlyList<RecentUserDashboardDto> RecentUsers { get; set; } = [];
    public IReadOnlyList<ChartPointDto> MonthlyAppointments { get; set; } = [];
    public IReadOnlyList<ChartPointDto> MonthlyRevenuePoints { get; set; } = [];
    public IReadOnlyList<TopDoctorDashboardDto> TopDoctors { get; set; } = [];
}

public sealed class DoctorDashboardViewModel
{
    public string DoctorId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? ProfilePicture { get; set; }
    public string Specialization { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public bool IsAvailable { get; set; }
    public int TodaysAppointmentsCount { get; set; }
    public int UpcomingAppointmentsCount { get; set; }
    public int TotalPatients { get; set; }
    public int PendingRequests { get; set; }
    public int CompletedSessions { get; set; }
    public decimal Earnings { get; set; }
    public IReadOnlyList<RecentAppointmentDashboardDto> TodaysAppointments { get; set; } = [];
    public IReadOnlyList<RecentAppointmentDashboardDto> UpcomingAppointments { get; set; } = [];
    public IReadOnlyList<DoctorScheduleDashboardDto> Schedules { get; set; } = [];
    public IReadOnlyList<DoctorClinicDashboardDto> Clinics { get; set; } = [];
}

public sealed class DoctorScheduleDashboardDto
{
    public int Id { get; set; }
    public DayOfWeek Day { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int SlotDurationMinutes { get; set; }
}

public sealed class DoctorClinicDashboardDto
{
    public int ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public decimal ConsultationFee { get; set; }
}

public sealed class PatientDashboardViewModel
{
    public string PatientId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfilePicture { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? BloodType { get; set; }
    public int ProfileCompletion { get; set; }
    public int UpcomingAppointmentsCount { get; set; }
    public int AppointmentHistoryCount { get; set; }
    public int PrescriptionCount { get; set; }
    public int MedicalRecordCount { get; set; }
    public int FavoriteDoctorsCount { get; set; }
    public IReadOnlyList<RecentAppointmentDashboardDto> UpcomingAppointments { get; set; } = [];
    public IReadOnlyList<RecentAppointmentDashboardDto> AppointmentHistory { get; set; } = [];
    public IReadOnlyList<PatientPrescriptionDashboardDto> Prescriptions { get; set; } = [];
    public IReadOnlyList<PatientMedicalSummaryDto> MedicalSummaries { get; set; } = [];
}

public sealed class PatientPrescriptionDashboardDto
{
    public int Id { get; set; }
    public DateTime PrescriptionDate { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public int ItemsCount { get; set; }
    public string? Notes { get; set; }
}

public sealed class PatientMedicalSummaryDto
{
    public int Id { get; set; }
    public string Condition { get; set; } = string.Empty;
    public DateTime DiagnosedDate { get; set; }
    public bool IsActive { get; set; }
}
