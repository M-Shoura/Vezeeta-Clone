using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Presentation.ViewModels
{
    /// <summary>
    /// ViewModel used by admin when creating a new patient record.
    /// The ApplicationUserId must correspond to an existing ApplicationUser.
    /// </summary>
    public class CreatePatientViewModel
    {
        [Required(ErrorMessage = "Application User ID is required.")]
        [Display(Name = "Application User Id")]
        public string ApplicationUserId { get; set; } = string.Empty;

        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-25);

        [Required]
        public Gender Gender { get; set; }

        [Display(Name = "Blood Type")]
        public string? BloodType { get; set; }

        [Display(Name = "Emergency Contact Name")]
        public string? EmergencyContactName { get; set; }

        [Display(Name = "Emergency Contact Phone")]
        public string? EmergencyContactPhone { get; set; }
    }
}
