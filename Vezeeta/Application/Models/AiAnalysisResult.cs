using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Application.Models
{
    public class AiAnalysisResult
    {
        [JsonPropertyName("symptoms")]
        public List<string> Symptoms { get; set; } = new();

        [JsonPropertyName("diagnosticSuggestions")]
        public string DiagnosticSuggestions { get; set; } = string.Empty;
    }
}
