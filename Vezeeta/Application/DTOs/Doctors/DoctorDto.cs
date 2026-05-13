using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Doctors
{
    public class DoctorDto
    {
        public string Id { get; set; } = null!;

        public string FullName { get; set; } = null!;

        public string? ProfilePicture { get; set; }

        public string? Email { get; set; }

        public string Specialization { get; set; } = null!;

        public int YearsOfExperience { get; set; }

        public string? Bio { get; set; }

        public string Qualification { get; set; } = null!;

        public bool IsAvailable { get; set; }

        public string? LicenseNumber { get; set; }
    }
}
