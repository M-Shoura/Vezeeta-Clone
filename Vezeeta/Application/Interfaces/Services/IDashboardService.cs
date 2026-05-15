using Application.DTOs.Dashboards;

namespace Application.Interfaces.Services;

public interface IDashboardService
{
    Task<AdminDashboardViewModel> GetAdminDashboardAsync(CancellationToken cancellationToken = default);
    Task<DoctorDashboardViewModel?> GetDoctorDashboardAsync(string doctorUserId, CancellationToken cancellationToken = default);
    Task<PatientDashboardViewModel?> GetPatientDashboardAsync(string patientUserId, CancellationToken cancellationToken = default);
}
