using Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    /// <summary>
    /// Tracks patient medical history including diagnoses, conditions, and treatment outcomes
    /// </summary>
    public class MedicalRecord : AuditableEntity
    {
        public int PatientId { get; set; }
        public string Condition { get; set; } = null!;
        public string? Description { get; set; }
        public string? DiagnosisCode { get; set; } // ICD-10 code
        public DateTime DiagnosedDate { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Treatment { get; set; }
        public int? AppointmentId { get; set; }

        public PatientProfile Patient { get; set; } = null!;
        public Appointment? Appointment { get; set; }
    }
}