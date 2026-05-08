using Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class DoctorProfile : AuditableEntity
    {
        public string ApplicationUserId { get; set; } = null!;
        public string Specialization { get; set; } = null!;
        public int YearsOfExperience { get; set; }
        public decimal ConsultationFee { get; set; }
        public string? Bio { get; set; }
        public string Qualification { get; set; } = null!;
        public bool IsAvailable { get; set; } = true;
        public string? ProfileImage { get; set; }
        public string? LicenseNumber { get; set; }

        public ICollection<Clinic> Clinics { get; set; } = new HashSet<Clinic>();
        public ICollection<Appointment> Appointments { get; set; } = new HashSet<Appointment>();
        public ICollection<DoctorSchedule> Schedules { get; set; } = new HashSet<DoctorSchedule>();
        public ICollection<Review> Reviews { get; set; } = new HashSet<Review>();
    }
}
