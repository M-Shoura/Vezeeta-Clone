using System.ComponentModel.DataAnnotations;

namespace Presentation.ViewModels
{
    public class EditDoctorViewModel
    {
        public string ApplicationUserId { get; set; } = null!;

        [Required]
        public string FullName { get; set; } = null!;

        public string? ProfilePicture { get; set; }

        [Required]
        public string Specialization { get; set; } = null!;

        [Required]
        public int YearsOfExperience { get; set; }

        public string? Bio { get; set; }

        [Required]
        public string Qualification { get; set; } = null!;

        public bool IsAvailable { get; set; }

        public string? LicenseNumber { get; set; }
    }
}
