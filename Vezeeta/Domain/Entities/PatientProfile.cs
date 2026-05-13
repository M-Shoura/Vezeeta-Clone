using Domain.Entities.Common;
using Domain.Enums;
using Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class PatientProfile
    {
        public string ApplicationUserId { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public int Age => DateTime.Today.Year - DateOfBirth.Year - (DateTime.Today.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
        public Gender Gender { get; set; }
        public string? BloodType { get; set; }
        public string EmergencyContactName { get; set; } = null!;
        public string EmergencyContactPhone { get; set; } = null!;

        public ApplicationUser ApplicationUser { get; set; } = null!;
        public ICollection<Appointment> Appointments { get; set; } = new HashSet<Appointment>();
        public ICollection<MedicalRecord> MedicalRecords { get; set; } = new HashSet<MedicalRecord>();
        public ICollection<Review> Reviews { get; set; } = new HashSet<Review>();
    }
}
