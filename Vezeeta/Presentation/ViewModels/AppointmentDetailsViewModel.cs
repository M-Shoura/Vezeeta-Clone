using Application.DTOs.Dashboards;
using Application.DTOs.Medical_Records;
using Application.DTOs.Profiles.Patients;

namespace Presentation.ViewModels
{
    public class AppointmentDetailsViewModel
    {
        public RecentAppointmentDashboardDto? Appointment { get; set; }
        public PatientDto? Patient { get; set; }
        public IEnumerable<MedicalRecordDto> MedicalRecords { get; set; } = new List<MedicalRecordDto>();
    }
}
