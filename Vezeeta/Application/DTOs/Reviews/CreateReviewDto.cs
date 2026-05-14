using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

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
        public string DoctorId { get; set; } = null!;

        [Required]
        public string PatientId { get; set; } = null!;
    }
}
