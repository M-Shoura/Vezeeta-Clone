using System.ComponentModel.DataAnnotations;

namespace Presentation.ViewModels
{
    public class BookAppointmentViewModel
    {
        [Required]
        public string DoctorId { get; set; } = null!;

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        public int ClinicId { get; set; }

        public string? Notes { get; set; }
    }
}
