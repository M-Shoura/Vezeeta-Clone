using Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class MedicalRecord : AuditableEntity
    {
        public string PatientId { get; set; } = null!;
        public string Condition { get; set; } = null!;
        public string? Description { get; set; }
        public string? DiagnosisCode { get; set; }
        public DateTime DiagnosedDate { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Treatment { get; set; }
        public string? Allergies { get; set; }
        public string? Medications { get; set; }
        public string? SurgeryDetails { get; set; }
        public string? FamilyHistory { get; set; }
        public int? AppointmentId { get; set; }


        public PatientProfile Patient { get; set; } = null!;
        public Appointment? Appointment { get; set; }

        public override string ToString()
        {
            var activeStatus = IsActive ? "Active" : "Resolved";
            var resolvedStr = ResolvedDate.HasValue ? ResolvedDate.Value.ToString("yyyy-MM-dd") : "N/A";

            return $"[Medical Record] Condition: {Condition} | " +
                   $"Status: {activeStatus} | " +
                   $"Code: {DiagnosisCode ?? "None"} | " +
                   $"Diagnosed: {DiagnosedDate:yyyy-MM-dd} | " +
                   $"Resolved: {resolvedStr} | " +
                   $"Medications: {Medications ?? "None"} | " +
                   $"Allergies: {Allergies ?? "None"} | " +
                   $"Treatment: {Treatment ?? "None"} | " +
                   $"Surgery: {SurgeryDetails ?? "None"} | " +
                   $"Family History: {FamilyHistory ?? "None"} | " +
                   $"Appt ID: {AppointmentId?.ToString() ?? "None"} | " +
                   $"Description: {Description ?? "No description available."}";
        }
    }
}