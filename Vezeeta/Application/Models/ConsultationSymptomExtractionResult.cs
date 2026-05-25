using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Application.Models
{
    public class ConsultationSymptomExtractionResult
    {
        [JsonPropertyName("transcript")]
        public string Transcript { get; set; } = string.Empty;

        [JsonPropertyName("aiExtractedSymptoms")]
        public List<ClinicalSymptomItem> AiExtractedSymptoms { get; set; } = new();

        [JsonPropertyName("doctorObservations")]
        public List<string> DoctorObservations { get; set; } = new();

        [JsonPropertyName("currentMedications")]
        public List<string> CurrentMedications { get; set; } = new();

        [JsonPropertyName("pastMedicalHistory")]
        public List<string> PastMedicalHistory { get; set; } = new();

        [JsonPropertyName("chronicDiseases")]
        public List<string> ChronicDiseases { get; set; } = new();

        [JsonPropertyName("allergies")]
        public List<string> Allergies { get; set; } = new();

        [JsonPropertyName("familyHistory")]
        public List<string> FamilyHistory { get; set; } = new();

        [JsonPropertyName("lifestyleFactors")]
        public List<string> LifestyleFactors { get; set; } = new();

        [JsonPropertyName("vitalSigns")]
        public List<string> VitalSigns { get; set; } = new();

        [JsonPropertyName("patientConcerns")]
        public List<string> PatientConcerns { get; set; } = new();

        [JsonPropertyName("normalizationNotes")]
        public List<string> NormalizationNotes { get; set; } = new();
    }

    public class ClinicalSymptomItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("onset")]
        public string Onset { get; set; } = string.Empty;

        [JsonPropertyName("duration")]
        public string Duration { get; set; } = string.Empty;

        [JsonPropertyName("severity")]
        public string Severity { get; set; } = string.Empty;

        [JsonPropertyName("frequency")]
        public string Frequency { get; set; } = string.Empty;

        [JsonPropertyName("progression")]
        public string Progression { get; set; } = string.Empty;

        [JsonPropertyName("location")]
        public string Location { get; set; } = string.Empty;

        [JsonPropertyName("triggers")]
        public string Triggers { get; set; } = string.Empty;

        [JsonPropertyName("relievingFactors")]
        public string RelievingFactors { get; set; } = string.Empty;

        [JsonPropertyName("associatedSymptoms")]
        public string AssociatedSymptoms { get; set; } = string.Empty;

        [JsonPropertyName("source")]
        public string Source { get; set; } = "AI";
    }
}
