using Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class DoctorProfile
    {
        public int ApplicationUserId { get; set; }
        public string Specialization { get; set; } = null!;
        public int YearsOfExperience { get; set; }
        public string? Bio { get; set; }
        public string Qualification { get; set; } = null!;
        public bool IsAvailable { get; set; } = true;
        public string? LicenseNumber { get; set; }

        public ICollection<DoctorClinic> DoctorClinics { get; set; } = new HashSet<DoctorClinic>();
        public ICollection<Appointment> Appointments { get; set; } = new HashSet<Appointment>();
        public ICollection<DoctorSchedule> DoctorSchedules { get; set; } = new HashSet<DoctorSchedule>();
        public ICollection<Review> Reviews { get; set; } = new HashSet<Review>();
    }
}
