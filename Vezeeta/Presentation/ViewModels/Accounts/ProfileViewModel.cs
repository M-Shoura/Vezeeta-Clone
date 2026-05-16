using Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Presentation.ViewModels.Accounts
{
    public class ProfileViewModel
    {
        [Required]
        public string FullName { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Phone]
        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }
        public string? ProfilePicture { get; set; }
        public IFormFile? ProfilePictureFile { get; set; }

        [Required]
        public UserRole Role { get; set; }

        public DateTime? BirthDate { get; set; }
        public Gender? Gender { get; set; }
        public string? BloodType { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }

        public string? Specialization { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? Bio { get; set; }
        public string? Qualification { get; set; }
        public bool IsAvailable { get; set; }
        public string? LicenseNumber { get; set; }
    }
}
