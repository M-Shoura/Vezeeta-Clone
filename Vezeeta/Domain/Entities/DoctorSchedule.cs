using Domain.Entities.Common;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class DoctorSchedule : AuditableEntity
    {
        public DayOfWeek Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int SlotDurationMinutes { get; set; }

        public string DoctorProfileId { get; set; }
        public int ClinicId { get; set; }

        public DoctorProfile Doctor { get; set; }
        public Clinic Clinic { get; set; } = null!;
    }
}
