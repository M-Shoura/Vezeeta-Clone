using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class DoctorScheduleDTO
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public DayOfWeek Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int SlotDurationMinutes { get; set; }

        public bool IsActive { get; set; } = true;

        public string DoctorId { get; set; } = null!;
        public int ClinicId { get; set; }

        public DoctorProfile? Doctor { get; set; } = null!;
        public Clinic? Clinic { get; set; } = null!;
    }
}
