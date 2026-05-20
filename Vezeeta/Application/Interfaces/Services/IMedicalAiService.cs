using Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services
{
    public interface IMedicalAiService
    {
        Task<string> TranscribeAudioAsync(MemoryStream audioStream);
        Task<AiAnalysisResult> AnalyzeConsultationAsync(string transcript,string appointmentId);
    }
}
