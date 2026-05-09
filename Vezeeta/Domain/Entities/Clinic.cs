using Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class Clinic : AuditableEntity
    {
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? Description { get; set; }
        public string? Email { get; set; }
        
        public bool IsActive { get; set; } = true;

        public ICollection<DoctorProfile> Doctors { get; set; } = new HashSet<DoctorProfile>();
        public ICollection<DoctorClinic> DoctorClinics { get; set; } = new HashSet<DoctorClinic>();
        public ICollection<DoctorSchedule> Schedules { get; set; } = new HashSet<DoctorSchedule>();
        public ICollection<Appointment> Appointments { get; set; } = new HashSet<Appointment>();
    }
}
