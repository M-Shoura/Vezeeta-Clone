using System.Collections.Generic;

namespace Presentation.ViewModels
{
    public class PatientDashboardViewModel
    {
        public Application.DTOs.Profiles.Patients.PatientProfileDto Profile { get; set; } = null!;

        public List<Presentation.ViewModels.RecentAppointmentViewModel> Appointments { get; set; } = new();
    }
}
