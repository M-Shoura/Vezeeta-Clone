using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Application.Models
{
    public class PrescriptionSafetyReviewResult
    {
        [JsonPropertyName("hasIssues")]
        public bool HasIssues { get; set; }

        [JsonPropertyName("overallRiskLevel")]
        public string OverallRiskLevel { get; set; } = "Low";

        [JsonPropertyName("summary")]
        public string Summary { get; set; } = string.Empty;

        [JsonPropertyName("issues")]
        public List<PrescriptionSafetyIssue> Issues { get; set; } = new();
    }

    public class PrescriptionSafetyIssue
    {
        [JsonPropertyName("riskLevel")]
        public string RiskLevel { get; set; } = "Low";

        [JsonPropertyName("problematicDrugs")]
        public List<string> ProblematicDrugs { get; set; } = new();

        [JsonPropertyName("medicalReasoning")]
        public string MedicalReasoning { get; set; } = string.Empty;

        [JsonPropertyName("relatedPatientRecordOrCondition")]
        public string RelatedPatientRecordOrCondition { get; set; } = string.Empty;

        [JsonPropertyName("suggestedSaferAlternatives")]
        public List<string> SuggestedSaferAlternatives { get; set; } = new();

        [JsonPropertyName("clinicalRecommendation")]
        public string ClinicalRecommendation { get; set; } = string.Empty;
    }
}
