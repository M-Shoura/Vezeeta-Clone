using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Application.Models
{
    public class ConsultationDiagnosticResult
    {
        [JsonPropertyName("isClinicalInformationSufficient")]
        public bool IsClinicalInformationSufficient { get; set; }

        [JsonPropertyName("mode")]
        public string Mode { get; set; } = "ClinicalInformationRequired";

        [JsonPropertyName("patientOverview")]
        public ClinicalPatientOverview PatientOverview { get; set; } = new();

        [JsonPropertyName("confirmedClinicalData")]
        public ConfirmedClinicalData ConfirmedClinicalData { get; set; } = new();

        [JsonPropertyName("missingInformation")]
        public List<MissingClinicalInformation> MissingInformation { get; set; } = new();

        [JsonPropertyName("targetedFollowUpQuestions")]
        public List<string> TargetedFollowUpQuestions { get; set; } = new();

        [JsonPropertyName("clinicalJustification")]
        public List<string> ClinicalJustification { get; set; } = new();

        [JsonPropertyName("possibleDiagnoses")]
        public List<DiagnosticCandidate> PossibleDiagnoses { get; set; } = new();

        [JsonPropertyName("differentialDiagnoses")]
        public List<DiagnosticCandidate> DifferentialDiagnoses { get; set; } = new();

        [JsonPropertyName("redFlagWarnings")]
        public List<string> RedFlagWarnings { get; set; } = new();

        [JsonPropertyName("recommendedTests")]
        public RecommendedTests RecommendedTests { get; set; } = new();

        [JsonPropertyName("clinicalPlan")]
        public List<string> ClinicalPlan { get; set; } = new();

        [JsonPropertyName("uncertaintyAnalysis")]
        public string UncertaintyAnalysis { get; set; } = string.Empty;
    }

    public class ClinicalPatientOverview
    {
        [JsonPropertyName("age")]
        public string Age { get; set; } = string.Empty;

        [JsonPropertyName("gender")]
        public string Gender { get; set; } = string.Empty;

        [JsonPropertyName("keyHistory")]
        public List<string> KeyHistory { get; set; } = new();

        [JsonPropertyName("chronicDiseases")]
        public List<string> ChronicDiseases { get; set; } = new();

        [JsonPropertyName("allergies")]
        public List<string> Allergies { get; set; } = new();
    }

    public class ConfirmedClinicalData
    {
        [JsonPropertyName("symptoms")]
        public List<ClinicalSymptomItem> Symptoms { get; set; } = new();

        [JsonPropertyName("physicalExaminationFindings")]
        public List<ClinicalSymptomItem> PhysicalExaminationFindings { get; set; } = new();

        [JsonPropertyName("vitalSigns")]
        public List<string> VitalSigns { get; set; } = new();

        [JsonPropertyName("doctorNotes")]
        public string DoctorNotes { get; set; } = string.Empty;
    }

    public class MissingClinicalInformation
    {
        [JsonPropertyName("field")]
        public string Field { get; set; } = string.Empty;

        [JsonPropertyName("reason")]
        public string Reason { get; set; } = string.Empty;

        [JsonPropertyName("urgency")]
        public string Urgency { get; set; } = string.Empty;
    }

    public class DiagnosticCandidate
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("confidenceLevel")]
        public string ConfidenceLevel { get; set; } = string.Empty;

        [JsonPropertyName("confidenceScore")]
        public int ConfidenceScore { get; set; }

        [JsonPropertyName("supportingSymptoms")]
        public List<string> SupportingSymptoms { get; set; } = new();

        [JsonPropertyName("contradictingSymptoms")]
        public List<string> ContradictingSymptoms { get; set; } = new();

        [JsonPropertyName("clinicalReasoning")]
        public string ClinicalReasoning { get; set; } = string.Empty;
    }

    public class RecommendedTests
    {
        [JsonPropertyName("laboratory")]
        public List<string> Laboratory { get; set; } = new();

        [JsonPropertyName("imaging")]
        public List<string> Imaging { get; set; } = new();

        [JsonPropertyName("monitoring")]
        public List<string> Monitoring { get; set; } = new();
    }
}
