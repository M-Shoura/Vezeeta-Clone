using Application.DTOs.Prescriptions;
using Application.Models;

namespace Application.Interfaces.Services
{
    public interface IMedicalAiService
    {
        Task<string> TranscribeAudioAsync(MemoryStream audioStream);
        Task<AiAnalysisResult> AnalyzeConsultationAsync(string transcript, string appointmentId);
        Task<ConsultationSymptomExtractionResult> ExtractConsultationSymptomsAsync(string transcript, string appointmentId);
        Task<ConsultationSymptomExtractionResult> ExtractConsultationSnapshotAsync(string transcript, string appointmentId);
        Task<ConsultationDiagnosticResult> GenerateDiagnosticsAsync(ConsultationDiagnosticRequest request);
        Task<PrescriptionSafetyReviewResult> AnalyzePrescriptionSafetyAsync(PrescriptionDto prescription);
    }
}
