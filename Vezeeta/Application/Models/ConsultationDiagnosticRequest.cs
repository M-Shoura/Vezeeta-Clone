using System.Collections.Generic;

namespace Application.Models
{
    public class ConsultationDiagnosticRequest
    {
        public string AppointmentId { get; set; } = string.Empty;
        public List<ClinicalSymptomItem> AiExtractedSymptoms { get; set; } = new();
        public List<ClinicalSymptomItem> DoctorAddedSymptoms { get; set; } = new();
        public List<ClinicalSymptomItem> PhysicalExaminationFindings { get; set; } = new();
        public List<string> VitalSigns { get; set; } = new();
        public List<string> DoctorObservations { get; set; } = new();
        public List<string> CurrentMedications { get; set; } = new();
        public List<string> PastMedicalHistory { get; set; } = new();
        public List<string> ChronicDiseases { get; set; } = new();
        public List<string> Allergies { get; set; } = new();
        public List<string> FamilyHistory { get; set; } = new();
        public List<string> LifestyleFactors { get; set; } = new();
        public List<string> PatientConcerns { get; set; } = new();
        public string? DoctorNotes { get; set; }
        public string? Transcript { get; set; }
    }
}
