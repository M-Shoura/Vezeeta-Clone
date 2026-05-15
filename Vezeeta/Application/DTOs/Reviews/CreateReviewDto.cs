using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Application.Validators;

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

        [Required]
        [NotEmptyOrWhiteSpace]
        public string DoctorId { get; set; } = null!;

        [Required]
        [NotEmptyOrWhiteSpace]
        public string PatientId { get; set; } = null!;
    }
}
