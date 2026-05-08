using Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class Clinic : AuditableEntity
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Description { get; set; }

        public ICollection<DoctorProfile> Doctors { get; set; } = new HashSet<DoctorProfile>();
        public ICollection<DoctorSchedule> Schedules { get; set; } = new HashSet<DoctorSchedule>();
        public ICollection<Appointment> Appointments { get; set; } = new HashSet<Appointment>();
    }
}
