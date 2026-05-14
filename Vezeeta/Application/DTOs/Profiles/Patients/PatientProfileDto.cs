using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Profiles.Patients
{
    public class PatientProfileDto
    {
        public string FullName { get; set; } = null!;
        public string? ProfilePicture { get; set; }
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public DateTime BirthDate { get; set; }

        // public string Area { get; set; }
    }
}
