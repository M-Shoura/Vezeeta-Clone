using System.ComponentModel.DataAnnotations;

namespace Presentation.ViewModels.Reviews
{
    public class ReviewCreateViewModel
    {
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Comment { get; set; }

        [Required]
        public int AppointmentId { get; set; }

        [Required]
        public string DoctorId { get; set; } = null!;

        [Required]
        public string PatientId { get; set; } = null!;
    }
}
