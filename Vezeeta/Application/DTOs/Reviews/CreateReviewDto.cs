using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Reviews
{
    public class CreateReviewDto
    {
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Comment { get; set; }

        [Required]
        public int AppointmentId { get; set; }

        public string DoctorId { get; set; } = null!;
        public string PatientId { get; set; } = null!;
    }
}
