using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models;
using Domain.Entities;
using Domain.Enums;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Infrastructure.Services
{
    public class MedicalAiService : IMedicalAiService
    {
        private readonly HttpClient _httpClient;
        private readonly IGenericRepository<Appointment> _repository;

        private const string GroqApiKey = "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";

        public MedicalAiService(HttpClient httpClient,IUnitOfWork unitOfWork)
        {
            _httpClient = httpClient;
            _repository = unitOfWork.Repository<Appointment>();

        }

        // 1. Converts raw audio stream chunks to text transcript using Whisper
        // 1. Converts raw audio stream chunks to text transcript using Whisper
        // 1. Converts raw audio stream chunks to text transcript using Whisper
        public async Task<string> TranscribeAudioAsync(MemoryStream audioStream)
        {
            try
            {
                using var content = new MultipartFormDataContent();

                // Force an isolated, brand new memory allocation boundary for this request instance
                var audioBytes = audioStream.ToArray();
                var isolatedStream = new MemoryStream(audioBytes);

                var audioContent = new StreamContent(isolatedStream);
                audioContent.Headers.ContentType = MediaTypeHeaderValue.Parse("audio/webm");

                // Add variables matching Groq requirement specs
                content.Add(audioContent, "file", "consultation.webm");
                content.Add(new StringContent("whisper-large-v3"), "model");
                content.Add(new StringContent("ar"), "language");

                using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/audio/transcriptions");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GroqApiKey);
                request.Content = content;

                // CRITICAL FIXES FOR SEQUENTIAL UPLOADS:
                // 1. Force the request over HTTP/1.1 to bypass HTTP/2 stream multiplexing bugs
                request.Version = System.Net.HttpVersion.Version11;
                request.VersionPolicy = HttpVersionPolicy.RequestVersionExact;

                // 2. Add connection close header to prevent old multi-part headers leaking into this session
                request.Headers.ConnectionClose = true;

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorLog = await response.Content.ReadAsStringAsync();
                    return $"[Transcription Engine Error: {response.StatusCode} - {errorLog}]";
                }

                var jsonResult = await response.Content.ReadFromJsonAsync<JsonDocument>();
                return jsonResult?.RootElement.GetProperty("text").GetString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                return $"[Error communicating with free transcription service: {ex.Message}]";
            }
        }

        // 2. Processes the transcript with a specialized LLM using Structured JSON Output (Primary: ALLAM, Fallback: Llama 3.3)
        public async Task<AiAnalysisResult> AnalyzeConsultationAsync(string transcript,string appointmentId)
        {
            var appointment = _repository.Find(a => a.Id.ToString() == appointmentId, ["Patient"]);
            var age = appointment.Patient.Age;
            var gender = appointment.Patient.Gender.ToString();
            var medicalRecords = String.Join("\n", appointment.Patient.MedicalRecords.Select(m => m.ToString()).ToList());
            var structuredPrompt = $@"
 Role and Context:
You are an expert Clinical Decision Support AI acting as an elite peer consultant to the user, who is an Internal Medicine Specialist. 
Analyze the provided multi-modal clinical data payload (Patient Demographics, Longitudinal Medical Records, and a raw Speech-to-Text Transcript) to assist the specialist in formulating differential diagnoses.

User Profile: 
The user is an Internal Medicine Specialist. Do NOT include generic patient safety warnings, patronizing legal disclaimers, or phrases such as 'يرجى مراجعة طبيب مختص'. Speak in advanced, high-level clinical prose suitable for an attending physician.

Clinical Data Payload:
- PATIENT DEMOGRAPHICS: Age: {age}, Gender: {gender}.
- PATIENT MEDICAL HISTORY & RECORD SUMMARY: {medicalRecords}
- CURRENT CONSULTATION TRANSCRIPT:{transcript}
Execution Protocol:
1. Cross-reference the current vocal transcript symptoms against the patient's existing medical records and demographic baseline risk factors.
2. Extract all acute, explicit clinical symptoms discussed in the transcript.
3. Formulate high-probability differential diagnostic suggestions, clinical confidence intervals, and immediate high-yield diagnostic investigations (e.g., specific lab work, imaging studies) or intervention steps. Do not declare a singular definitive diagnosis.

Strict System Constraints:
- Return ONLY valid JSON matching the exact schema provided below.
- All text strings within the JSON properties must be written in professional medical Arabic (باللغة العربية الطبية الفصيحة).
- Do NOT output markdown code blocks (e.g., do not wrap in ```json ... ```), preambles, introductory text, or postscript remarks. 
- Output raw UTF-8 Arabic text inside the JSON values; do NOT use Unicode escape sequences (e.g., \uXXXX).
- Every array must contain ONLY strings. Do not use nested objects.
- Do NOT hallucinate or extrapolate symptoms not explicitly referenced in the transcript.

CRITICAL JSON SCHEMA REQUIREMENT:
{{
    ""symptoms"": [""عرض 1"", ""عرض 2""],
    ""diagnosticSuggestions"": ""اكتب هنا التشخيصات التفريقية المحتملة بناءً على التاريخ المرضي والوضع الحالي، ومستوى الثقة السريرية، والخطوات الاستقصائية التالية المقترحة بدقة طبية عالية من طبيب إلى طبيب.""
}}
";

            // Try with the primary specialized Arabic model: allam-2-7b
            try
            {
                return await ExecuteLlmCallAsync("allam-2-7b", structuredPrompt, 0.2);
            }
            catch (Exception primaryEx)
            {
                // Fallback layer: If ALLAM is throttled, down, or fails, failover to llama-3.3-70b-versatile
                try
                {
                    return await ExecuteLlmCallAsync("llama-3.3-70b-versatile", structuredPrompt, 0.1);
                }
                catch (Exception fallbackEx)
                {
                    return new AiAnalysisResult
                    {
                        DiagnosticSuggestions = $"Both AI core models failed processing data. Primary Error: {primaryEx.Message}. Fallback Error: {fallbackEx.Message}"
                    };
                }
            }
        }

        /// <summary>
        /// Isolated helper module to manage the HTTP network calls and payload processing directly against Groq.
        /// </summary>
        private async Task<AiAnalysisResult> ExecuteLlmCallAsync(string modelName, string prompt, double temperature)
        {
            var payload = new
            {
                model = modelName,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = temperature,
                response_format = new { type = "json_object" },
                stream = false
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GroqApiKey);
            request.Content = JsonContent.Create(payload);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorDetails = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Model {modelName} rejected payload with status {response.StatusCode}. Details: {errorDetails}");
            }

            var jsonResult = await response.Content.ReadFromJsonAsync<JsonDocument>();
            var assistantResponse = jsonResult?.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrEmpty(assistantResponse))
                return new AiAnalysisResult();

            // Strip out any accidental thought generation blocks before attempting deserialization
            if (assistantResponse.Contains("</thought>"))
            {
                assistantResponse = assistantResponse.Split("</thought>").Last().Trim();
            }

            return JsonSerializer.Deserialize<AiAnalysisResult>(assistantResponse.Trim()) ?? new AiAnalysisResult();
        }
    }
}