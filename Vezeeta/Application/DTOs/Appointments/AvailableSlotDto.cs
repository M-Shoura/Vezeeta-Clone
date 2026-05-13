using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Appointments
{
    public class AvailableSlotDto
    {
        public DateTime Date { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public int ClinicId { get; set; }
    }
}
