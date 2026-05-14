using System;
using System.Collections.Generic;

namespace Application.DTOs.Prescriptions
{
    public class PrescriptionDto
    {
        public int Id { get; set; }
        public DateTime PrescriptionDate { get; set; }
        public string? Notes { get; set; }
        public int? AppointmentId { get; set; }

        // For display
        public string? AppointmentInfo { get; set; }

        public List<PrescriptionItemDto> Items { get; set; } = new();
    }
}
