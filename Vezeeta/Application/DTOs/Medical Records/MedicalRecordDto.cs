using System;
using System.ComponentModel.DataAnnotations;
using Application.Validators;

namespace Application.DTOs.Medical_Records
{
    public class MedicalRecordDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Patient ID is required")]
        public string PatientId { get; set; } = null!;

        [Required(ErrorMessage = "Condition is required")]
        [StringLength(250, MinimumLength = 3, 
            ErrorMessage = "Condition must be between 3 and 250 characters")]
        public string Condition { get; set; } = null!;

        [StringLength(2000, 
            ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? Description { get; set; }

        [StringLength(50, 
            ErrorMessage = "Diagnosis Code cannot exceed 50 characters")]
        [RegularExpression(@"^[A-Z0-9\-\.]*$", 
            ErrorMessage = "Diagnosis Code must contain only uppercase letters, numbers, hyphens, and dots")]
        public string? DiagnosisCode { get; set; }

        [Required(ErrorMessage = "Diagnosed Date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Diagnosed Date")]
        public DateTime DiagnosedDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Resolved Date")]
        [DateGreaterThanOrEqual(nameof(DiagnosedDate), ErrorMessage = "Resolved Date cannot be earlier than Diagnosed Date")]
        public DateTime? ResolvedDate { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        [StringLength(2000, 
            ErrorMessage = "Treatment cannot exceed 2000 characters")]
        public string? Treatment { get; set; }

        [StringLength(1000, 
            ErrorMessage = "Allergies cannot exceed 1000 characters")]
        public string? Allergies { get; set; }

        [StringLength(1000, 
            ErrorMessage = "Medications cannot exceed 1000 characters")]
        public string? Medications { get; set; }

        [StringLength(5000, 
            ErrorMessage = "Surgery Details cannot exceed 5000 characters")]
        public string? SurgeryDetails { get; set; }

        [StringLength(5000, 
            ErrorMessage = "Family History cannot exceed 5000 characters")]
        public string? FamilyHistory { get; set; }

        [Display(Name = "Appointment ID")]
        public int? AppointmentId { get; set; }
    }
}
