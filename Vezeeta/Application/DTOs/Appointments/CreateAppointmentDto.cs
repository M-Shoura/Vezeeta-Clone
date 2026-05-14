using System.ComponentModel.DataAnnotations;
using Vezeeta.Domain.Enums;

namespace Vezeeta.Application.DTOs.Appointments
{
    public class CreateAppointmentDto
    {
        [Required(ErrorMessage = "Appointment date is required")]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Start time is required")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        [Required(ErrorMessage = "Doctor is required")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Clinic is required")]
        public int ClinicId { get; set; }

        [Required(ErrorMessage = "Patient is required")]
        public int PatientId { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }
}
