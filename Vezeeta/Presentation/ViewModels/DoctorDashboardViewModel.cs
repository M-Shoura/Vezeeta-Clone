using Application.DTOs.Doctors;
using Domain.Entities;

namespace Presentation.ViewModels
{
    public class DoctorDashboardViewModel
    {
        public DoctorDetailsDto Doctor { get; set; } = null!;
        public IEnumerable<Appointment> Appointments { get; set; } = new List<Appointment>();
        public IEnumerable<DoctorClinic> Clinics { get; set; } = new List<DoctorClinic>();
    }
}
