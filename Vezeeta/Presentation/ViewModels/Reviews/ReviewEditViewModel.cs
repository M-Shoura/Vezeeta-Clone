using System.ComponentModel.DataAnnotations;
using Application.Validators;

namespace Presentation.ViewModels.Reviews
{
    public class ReviewEditViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [NotEmptyOrWhiteSpace]
        public string? Comment { get; set; }

        [Required]
        public int AppointmentId { get; set; }

        [Required]
        public string DoctorId { get; set; } = null!;

        [Required]
        public string PatientId { get; set; } = null!;
    }
}
