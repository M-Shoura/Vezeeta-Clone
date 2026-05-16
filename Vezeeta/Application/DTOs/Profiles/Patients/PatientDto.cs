using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Profiles.Patients
{
    public class PatientDto
    {
        public string ApplicationUserId { get; set; }
        public string FullName { get; set; } = null!;
        public string? ProfilePicture { get; set; }
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public DateTime BirthDate { get; set; }

        public DateTime DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public string? BloodType { get; set; }
        public string EmergencyContactName { get; set; } = null!;
        public string EmergencyContactPhone { get; set; } = null!;
    }
}
