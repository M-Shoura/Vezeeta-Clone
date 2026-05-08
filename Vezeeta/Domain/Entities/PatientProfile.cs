using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class PatientProfile
    {
        public string ApplicationUserId { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int Age => DateTime.Today.Year - DateOfBirth.Year - (DateTime.Today.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
        public Gender Gender { get; set; }
        public string BloodType { get; set; }
        public string EmergencyContactName { get; set; }
        public string EmergencyContactPhone { get; set; }
        public string? Allergies { get; set; }
        public string? ChronicDiseases { get; set; }
        public string? PreviousSurgeries { get; set; }
        public string? FamilyHistory { get; set; }
        public string? CurrentMedications { get; set; }

        public ICollection<Appointment> Appointments { get; set; } = new HashSet<Appointment>();
    }
}
