using Vezeeta.Domain.Enums;

namespace Vezeeta.Application.DTOs.Appointments
{
    public class AppointmentDto
    {
        public int Id { get; set; }

        public DateTime AppointmentDate { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public AppointmentStatus Status { get; set; }

        public int DoctorId { get; set; }

        public string? DoctorName { get; set; }

        public int PatientId { get; set; }

        public string? PatientName { get; set; }

        public int ClinicId { get; set; }

        public string? ClinicName { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedOn { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? LastModifiedOn { get; set; }

        public string? LastModifiedBy { get; set; }
    }
}
