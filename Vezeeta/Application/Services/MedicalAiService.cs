using Application.DTOs.Prescriptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models;
using Domain.Entities;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Infrastructure.Services
{
    public class MedicalAiService : IMedicalAiService
    {
        private readonly HttpClient _httpClient;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<Appointment> _repository;

        private const string GroqApiKey = "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";

        public MedicalAiService(HttpClient httpClient, IUnitOfWork unitOfWork)
        {
            _httpClient = httpClient;
            _unitOfWork = unitOfWork;
            _repository = unitOfWork.Repository<Appointment>();
        }

        public async Task<string> TranscribeAudioAsync(MemoryStream audioStream)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                var isolatedStream = new MemoryStream(audioStream.ToArray());
                var audioContent = new StreamContent(isolatedStream);
                audioContent.Headers.ContentType = MediaTypeHeaderValue.Parse("audio/webm");

                content.Add(audioContent, "file", "consultation.webm");
                content.Add(new StringContent("whisper-large-v3"), "model");
                content.Add(new StringContent("ar"), "language");

                using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/audio/transcriptions");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GroqApiKey);
                request.Content = content;
                request.Version = System.Net.HttpVersion.Version11;
                request.VersionPolicy = HttpVersionPolicy.RequestVersionExact;
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

        public async Task<AiAnalysisResult> AnalyzeConsultationAsync(string transcript, string appointmentId)
        {
            var extraction = await ExtractConsultationSymptomsAsync(transcript, appointmentId);

            return new AiAnalysisResult
            {
                Symptoms = extraction.AiExtractedSymptoms.Select(s => s.Name).Where(s => !string.IsNullOrWhiteSpace(s)).ToList(),
                DiagnosticSuggestions = "Symptoms were extracted only. Confirm the validated clinical dataset before diagnostic generation."
            };
        }

        public async Task<ConsultationSymptomExtractionResult> ExtractConsultationSymptomsAsync(string transcript, string appointmentId)
        {
            return await ExtractConsultationFactsAsync(transcript, appointmentId, "full audited transcript", 0.1);
        }

        public async Task<ConsultationSymptomExtractionResult> ExtractConsultationSnapshotAsync(string transcript, string appointmentId)
        {
            return await ExtractConsultationFactsAsync(transcript, appointmentId, "live partial transcript snapshot", 0);
        }

        public async Task<ConsultationDiagnosticResult> GenerateDiagnosticsAsync(ConsultationDiagnosticRequest request)
        {
            var appointment = await GetAppointmentClinicalContextAsync(request.AppointmentId);
            var prompt = BuildDiagnosticPrompt(request, appointment);

            try
            {
                return EnforceSufficiency(await ExecuteJsonLlmCallAsync<ConsultationDiagnosticResult>("llama-3.3-70b-versatile", prompt, 0.1));
            }
            catch
            {
                return EnforceSufficiency(await ExecuteJsonLlmCallAsync<ConsultationDiagnosticResult>("allam-2-7b", prompt, 0.1));
            }
        }

        public async Task<PrescriptionSafetyReviewResult> AnalyzePrescriptionSafetyAsync(PrescriptionDto prescription)
        {
            if (prescription.AppointmentId == null || prescription.AppointmentId <= 0)
                return BuildBlockingSafetyResult("Prescription safety review cannot run without a valid appointment.", "Missing appointment reference");

            var appointment = await _repository.FindAsync(a => a.Id == prescription.AppointmentId.Value, new[]
            {
                "Patient",
                "Patient.ApplicationUser",
                "Patient.MedicalRecords",
                "Prescription",
                "Prescription.PrescriptionItems",
                "Prescription.PrescriptionItems.Drug"
            });

            if (appointment?.Patient == null)
                return BuildBlockingSafetyResult("Prescription safety review cannot run because the patient profile was not found.", "Missing patient profile");

            await AttachDrugNamesAsync(prescription);

            var priorPrescriptions = await _unitOfWork.Repository<Prescription>().FindAllAsync(
                p => p.Appointment.PatientId == appointment.PatientId && p.AppointmentId != appointment.Id,
                includes: new[] { "Appointment", "PrescriptionItems", "PrescriptionItems.Drug" });

            var localIssues = BuildLocalPrescriptionIssues(prescription);
            var prompt = BuildPrescriptionSafetyPrompt(prescription, appointment, priorPrescriptions);
            PrescriptionSafetyReviewResult aiResult;

            try
            {
                aiResult = await ExecuteJsonLlmCallAsync<PrescriptionSafetyReviewResult>("llama-3.3-70b-versatile", prompt, 0.1);
            }
            catch (Exception primaryEx)
            {
                try
                {
                    aiResult = await ExecuteJsonLlmCallAsync<PrescriptionSafetyReviewResult>("allam-2-7b", prompt, 0.1);
                }
                catch (Exception fallbackEx)
                {
                    aiResult = new PrescriptionSafetyReviewResult
                    {
                        HasIssues = true,
                        OverallRiskLevel = "High",
                        Summary = $"AI prescription safety review failed. Primary Error: {primaryEx.Message}. Fallback Error: {fallbackEx.Message}",
                        Issues =
                        {
                            new PrescriptionSafetyIssue
                            {
                                RiskLevel = "High",
                                MedicalReasoning = "The prescription could not be checked against the patient's longitudinal medical record by the AI safety reviewer.",
                                RelatedPatientRecordOrCondition = "AI safety review unavailable",
                                ClinicalRecommendation = "Manually verify allergies, interactions, contraindications, duplicate therapy, and dosing before approval."
                            }
                        }
                    };
                }
            }

            if (localIssues.Any())
            {
                aiResult.HasIssues = true;
                aiResult.Issues.AddRange(localIssues);
            }

            if (aiResult.Issues.Any())
                aiResult.HasIssues = true;

            aiResult.OverallRiskLevel = NormalizeOverallRisk(aiResult);
            return aiResult;
        }

        private async Task<ConsultationSymptomExtractionResult> ExtractConsultationFactsAsync(
            string transcript,
            string appointmentId,
            string transcriptType,
            double temperature)
        {
            var appointment = await GetAppointmentClinicalContextAsync(appointmentId);
            var prompt = BuildExtractionPrompt(transcript, appointment, transcriptType);

            try
            {
                var result = await ExecuteJsonLlmCallAsync<ConsultationSymptomExtractionResult>("llama-3.3-70b-versatile", prompt, temperature);
                result.Transcript = string.IsNullOrWhiteSpace(result.Transcript) ? transcript : result.Transcript;
                return result;
            }
            catch
            {
                var result = await ExecuteJsonLlmCallAsync<ConsultationSymptomExtractionResult>("allam-2-7b", prompt, temperature);
                result.Transcript = string.IsNullOrWhiteSpace(result.Transcript) ? transcript : result.Transcript;
                return result;
            }
        }

        private static string BuildExtractionPrompt(string transcript, Appointment appointment, string transcriptType)
        {
            return $@"
Role:
You are a medical ambient-listening assistant. Your only job is extracting and normalizing clinical facts from a doctor-patient conversation.

Strict prohibitions:
- Do not generate diagnoses.
- Do not generate differential diagnoses.
- Do not recommend treatment.
- Do not infer facts from keywords.
- If a field is not stated, leave it empty.

Patient context:
- Age: {appointment.Patient.Age}
- Gender: {appointment.Patient.Gender}
- Blood type: {appointment.Patient.BloodType ?? "Unknown"}
- Medical records:
{RecordsToPrompt(appointment.Patient.MedicalRecords)}

Conversation transcript type: {transcriptType}
Conversation transcript:
{transcript}

Extraction rules:
- Support Arabic, English, and mixed Arabic-English speech.
- Handle noisy or incomplete conversations conservatively.
- Normalize lay terms into clinically clear terms.
- Extract symptoms, onset, duration, severity, frequency, progression, location, triggers, relieving factors, associated symptoms, current medications, past history, chronic diseases, allergies, family history, lifestyle factors, vital signs, doctor observations, and patient concerns.
- Preserve the full transcript for audit.
- Return ONLY valid JSON:
{{
  ""transcript"": ""full transcript for audit"",
  ""aiExtractedSymptoms"": [
    {{
      ""name"": ""normalized symptom name"",
      ""onset"": ""when it started, empty if unknown"",
      ""duration"": ""duration, empty if unknown"",
      ""severity"": ""mild | moderate | severe | empty if unknown"",
      ""frequency"": ""frequency, empty if unknown"",
      ""progression"": ""improving | worsening | stable | empty if unknown"",
      ""location"": ""location, empty if unknown"",
      ""triggers"": ""triggers, empty if unknown"",
      ""relievingFactors"": ""relieving factors, empty if unknown"",
      ""associatedSymptoms"": ""associated symptoms, empty if unknown"",
      ""source"": ""AI""
    }}
  ],
  ""doctorObservations"": [""doctor observation""],
  ""currentMedications"": [""current medication""],
  ""pastMedicalHistory"": [""past medical history item""],
  ""chronicDiseases"": [""chronic disease""],
  ""allergies"": [""allergy""],
  ""familyHistory"": [""family history item""],
  ""lifestyleFactors"": [""lifestyle factor""],
  ""vitalSigns"": [""vital sign""],
  ""patientConcerns"": [""patient concern""],
  ""normalizationNotes"": [""normalization note""]
}}";
        }

        private static string BuildDiagnosticPrompt(ConsultationDiagnosticRequest request, Appointment appointment)
        {
            return $@"
Role:
You are an expert clinical decision-support AI. The doctor has validated the clinical dataset. Your workflow must enforce clinical sufficiency before diagnosis.

Patient context:
- Age: {appointment.Patient.Age}
- Gender: {appointment.Patient.Gender}
- Blood type: {appointment.Patient.BloodType ?? "Unknown"}
- Appointment notes: {appointment.Notes ?? "None"}
- Full patient medical history:
{RecordsToPrompt(appointment.Patient.MedicalRecords)}

Validated dataset:
This dataset is the ONLY permitted source for diagnostic reasoning. The transcript is audit-only and must not introduce any unvalidated fact.

AI-extracted symptoms validated or corrected by doctor:
{SymptomsToPrompt(request.AiExtractedSymptoms)}

Doctor additions:
{SymptomsToPrompt(request.DoctorAddedSymptoms)}

Physical examination findings and symptoms after physical examination:
{SymptomsToPrompt(request.PhysicalExaminationFindings)}

Vital signs:
{ListToPrompt(request.VitalSigns)}

Doctor observations:
{ListToPrompt(request.DoctorObservations)}

Current medications:
{ListToPrompt(request.CurrentMedications)}

Past medical history:
{ListToPrompt(request.PastMedicalHistory)}

Chronic diseases:
{ListToPrompt(request.ChronicDiseases)}

Allergies:
{ListToPrompt(request.Allergies)}

Family history:
{ListToPrompt(request.FamilyHistory)}

Lifestyle factors:
{ListToPrompt(request.LifestyleFactors)}

Patient concerns:
{ListToPrompt(request.PatientConcerns)}

Doctor notes and corrections:
{request.DoctorNotes ?? "None"}

Audit-only transcript:
{request.Transcript ?? "None"}

Stage 3 clinical sufficiency hard rule:
- Before generating ANY diagnosis, decide if the validated dataset is sufficient.
- If insufficient, set isClinicalInformationSufficient=false and mode=""ClinicalInformationRequired"".
- In ClinicalInformationRequired mode, possibleDiagnoses and differentialDiagnoses MUST be empty arrays.
- In ClinicalInformationRequired mode, provide missingInformation, targetedFollowUpQuestions, clinicalJustification, redFlagWarnings, patientOverview, confirmedClinicalData, and uncertaintyAnalysis.
- Never provide diagnosis names, possible conditions, differential labels, assumptions, or keyword-based inference until sufficient data exists.

Stage 4 diagnostic reasoning:
- Only if sufficient, set isClinicalInformationSufficient=true and mode=""DiagnosticReasoning"".
- Generate possible diagnoses, ranked differential diagnoses, confidence level and score, supporting and contradicting evidence, clinical reasoning, red flags, recommended tests grouped by laboratory/imaging/monitoring, clinical plan, and uncertainty analysis.
- Every diagnosis must be explainable and evidence-based.
- Avoid overconfidence.

Return ONLY valid JSON:
{{
  ""isClinicalInformationSufficient"": false,
  ""mode"": ""ClinicalInformationRequired | DiagnosticReasoning"",
  ""patientOverview"": {{
    ""age"": ""age"",
    ""gender"": ""gender"",
    ""keyHistory"": [""history""],
    ""chronicDiseases"": [""condition""],
    ""allergies"": [""allergy""]
  }},
  ""confirmedClinicalData"": {{
    ""symptoms"": [],
    ""physicalExaminationFindings"": [],
    ""vitalSigns"": [""vital sign""],
    ""doctorNotes"": ""notes""
  }},
  ""missingInformation"": [
    {{
      ""field"": ""missing clinical element"",
      ""reason"": ""why this matters medically"",
      ""urgency"": ""Low | Medium | High""
    }}
  ],
  ""targetedFollowUpQuestions"": [""focused clinical question""],
  ""clinicalJustification"": [""why a missing detail matters""],
  ""possibleDiagnoses"": [
    {{
      ""name"": ""diagnosis name"",
      ""confidenceLevel"": ""Low | Medium | High"",
      ""confidenceScore"": 0,
      ""supportingSymptoms"": [""supporting evidence""],
      ""contradictingSymptoms"": [""contradicting evidence""],
      ""clinicalReasoning"": ""evidence-based reasoning""
    }}
  ],
  ""differentialDiagnoses"": [
    {{
      ""name"": ""ranked differential diagnosis"",
      ""confidenceLevel"": ""Low | Medium | High"",
      ""confidenceScore"": 0,
      ""supportingSymptoms"": [""supporting evidence""],
      ""contradictingSymptoms"": [""contradicting evidence""],
      ""clinicalReasoning"": ""why this remains possible and what would distinguish it""
    }}
  ],
  ""redFlagWarnings"": [""urgent red flag warning""],
  ""recommendedTests"": {{
    ""laboratory"": [""lab test""],
    ""imaging"": [""imaging test""],
    ""monitoring"": [""monitoring or functional test""]
  }},
  ""clinicalPlan"": [""next clinical action""],
  ""uncertaintyAnalysis"": ""what is uncertain, why, and what data improves accuracy""
}}";
        }

        private async Task<T> ExecuteJsonLlmCallAsync<T>(string modelName, string prompt, double temperature)
            where T : new()
        {
            var payload = new
            {
                model = modelName,
                messages = new[] { new { role = "user", content = prompt } },
                temperature,
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
                throw new HttpRequestException($"Model {modelName} rejected JSON payload with status {response.StatusCode}. Details: {errorDetails}");
            }

            var jsonResult = await response.Content.ReadFromJsonAsync<JsonDocument>();
            var assistantResponse = jsonResult?.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(assistantResponse))
                return new T();

            return JsonSerializer.Deserialize<T>(
                CleanJsonResponse(assistantResponse),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new T();
        }

        private async Task<Appointment> GetAppointmentClinicalContextAsync(string appointmentId)
        {
            if (!int.TryParse(appointmentId, out var id))
                throw new ArgumentException("Invalid appointment id.", nameof(appointmentId));

            var appointment = await _repository.FindAsync(a => a.Id == id, new[]
            {
                "Patient",
                "Patient.ApplicationUser",
                "Patient.MedicalRecords"
            });

            if (appointment?.Patient == null)
                throw new InvalidOperationException("Appointment or patient clinical context was not found.");

            return appointment;
        }

        private async Task AttachDrugNamesAsync(PrescriptionDto prescription)
        {
            var drugIds = prescription.Items
                .Where(i => i.DrugId > 0)
                .Select(i => i.DrugId)
                .Distinct()
                .ToHashSet();

            if (drugIds.Count == 0)
                return;

            var drugs = await _unitOfWork.Repository<Drug>().FindAllAsync(d => drugIds.Contains(d.Id));
            var drugLookup = drugs.ToDictionary(d => d.Id);

            foreach (var item in prescription.Items)
            {
                if (drugLookup.TryGetValue(item.DrugId, out var drug))
                    item.DrugName = drug.Name;
            }
        }

        private static PrescriptionSafetyReviewResult BuildBlockingSafetyResult(string summary, string relatedRecord)
        {
            return new PrescriptionSafetyReviewResult
            {
                HasIssues = true,
                OverallRiskLevel = "High",
                Summary = summary,
                Issues =
                {
                    new PrescriptionSafetyIssue
                    {
                        RiskLevel = "High",
                        MedicalReasoning = "Medication safety checks require a complete patient context before clinical approval.",
                        RelatedPatientRecordOrCondition = relatedRecord,
                        ClinicalRecommendation = "Resolve the missing prescription context before approving the prescription."
                    }
                }
            };
        }

        private static string BuildPrescriptionSafetyPrompt(
            PrescriptionDto prescription,
            Appointment appointment,
            IEnumerable<Prescription> priorPrescriptions)
        {
            var patient = appointment.Patient;
            var activeRecords = patient.MedicalRecords.Where(r => r.IsActive).OrderByDescending(r => r.DiagnosedDate).ToList();
            var resolvedRecords = patient.MedicalRecords.Where(r => !r.IsActive).OrderByDescending(r => r.DiagnosedDate).ToList();
            var prescribedItems = prescription.Items.Select(item =>
                $"- Drug: {item.DrugName ?? $"DrugId {item.DrugId}"} | Dosage: {item.Dosage} | Times/day: {item.TimesPerDay} | Duration days: {item.DurationInDays} | Instructions: {item.Instructions ?? "None"}");
            var previousPrescriptionLines = priorPrescriptions
                .OrderByDescending(p => p.PrescriptionDate)
                .Take(10)
                .Select(p => $"Prescription {p.Id} on {p.PrescriptionDate:yyyy-MM-dd}: " +
                             string.Join("; ", p.PrescriptionItems.Select(i =>
                                 $"{i.Drug?.Name ?? $"DrugId {i.DrugId}"} {i.Dosage}, {i.TimesPerDay} times/day for {i.DurationInDays} days. Instructions: {i.Instructions ?? "None"}")))
                .ToList();

            return $@"
Role:
You are a clinical medication safety decision-support reviewer for a licensed physician. Review the draft prescription before final approval.

Clinical safety goal:
Detect drug-drug interactions, drug-disease contraindications, allergy conflicts, duplicate medications or duplicate therapeutic classes, unsafe or excessive dosages, age or condition restrictions, conflicts with existing treatment/history, and medications inappropriate for the diagnosed condition or patient status.

Patient demographics:
- Name: {patient.ApplicationUser?.FullName ?? "Unknown"}
- Age: {patient.Age}
- Gender: {patient.Gender}
- Blood type: {patient.BloodType ?? "Unknown"}

Current appointment:
- Date: {appointment.AppointmentDate:yyyy-MM-dd}
- Appointment notes / observations: {appointment.Notes ?? "None"}

Draft prescription context:
- Prescription notes / diagnosis / treatment notes / observations: {prescription.Notes ?? "None"}
- Medications:
{string.Join("\n", prescribedItems)}

Active medical records, diagnoses, chronic conditions, allergies, current medications, treatments, history, lab/result notes if present:
{RecordsToPrompt(activeRecords)}

Resolved or historical medical records:
{RecordsToPrompt(resolvedRecords)}

Previous prescriptions and existing treatment context:
{(previousPrescriptionLines.Any() ? string.Join("\n", previousPrescriptionLines) : "None recorded")}

Instructions:
- Use only the supplied patient and prescription data plus established clinical pharmacology knowledge.
- Be conservative and safety-focused.
- If no clinically meaningful issue is detected, set hasIssues to false, overallRiskLevel to Low, summary to a concise safe-to-review statement, and issues to [].
- If an issue is detected, include the problematic drug(s), detailed medical reasoning, the related patient record/condition, safer alternatives if available, and a clear clinical recommendation.
- Do not provide generic disclaimers.
- Return ONLY valid JSON matching this exact schema:
{{
  ""hasIssues"": true,
  ""overallRiskLevel"": ""Low | Medium | High | Critical"",
  ""summary"": ""Concise physician-facing safety summary."",
  ""issues"": [
    {{
      ""riskLevel"": ""Low | Medium | High | Critical"",
      ""problematicDrugs"": [""drug name""],
      ""medicalReasoning"": ""Explain the pharmacologic or clinical safety concern."",
      ""relatedPatientRecordOrCondition"": ""Specific allergy, diagnosis, lab/result note, current medication, previous prescription, age, pregnancy/condition status, or history item."",
      ""suggestedSaferAlternatives"": [""alternative or monitoring strategy""],
      ""clinicalRecommendation"": ""Actionable warning for the doctor before approval.""
    }}
  ]
}}";
        }

        private static ConsultationDiagnosticResult EnforceSufficiency(ConsultationDiagnosticResult result)
        {
            if (result.IsClinicalInformationSufficient)
                return result;

            result.Mode = "ClinicalInformationRequired";
            result.PossibleDiagnoses.Clear();
            result.DifferentialDiagnoses.Clear();
            return result;
        }

        private static string RecordsToPrompt(IEnumerable<MedicalRecord> records)
        {
            var lines = records.Select(record => record.ToString()).ToList();
            return lines.Any() ? string.Join("\n", lines) : "None recorded";
        }

        private static string ListToPrompt(IEnumerable<string> values)
        {
            var lines = values
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => $"- {value.Trim()}")
                .ToList();

            return lines.Any() ? string.Join("\n", lines) : "None recorded";
        }

        private static string SymptomsToPrompt(IEnumerable<ClinicalSymptomItem> symptoms)
        {
            var lines = symptoms
                .Where(symptom => !string.IsNullOrWhiteSpace(symptom.Name))
                .Select(symptom =>
                    $"- {symptom.Name.Trim()} | onset: {BlankToUnknown(symptom.Onset)} | duration: {BlankToUnknown(symptom.Duration)} | severity: {BlankToUnknown(symptom.Severity)} | frequency: {BlankToUnknown(symptom.Frequency)} | progression: {BlankToUnknown(symptom.Progression)} | location: {BlankToUnknown(symptom.Location)} | triggers: {BlankToUnknown(symptom.Triggers)} | relieving: {BlankToUnknown(symptom.RelievingFactors)} | associated: {BlankToUnknown(symptom.AssociatedSymptoms)} | source: {BlankToUnknown(symptom.Source)}")
                .ToList();

            return lines.Any() ? string.Join("\n", lines) : "None recorded";
        }

        private static string BlankToUnknown(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? "unknown" : value.Trim();
        }

        private static List<PrescriptionSafetyIssue> BuildLocalPrescriptionIssues(PrescriptionDto prescription)
        {
            var issues = new List<PrescriptionSafetyIssue>();

            var duplicateDrugNames = prescription.Items
                .Where(i => !string.IsNullOrWhiteSpace(i.DrugName))
                .GroupBy(i => i.DrugName!.Trim().ToLowerInvariant())
                .Where(g => g.Count() > 1)
                .Select(g => g.First().DrugName!)
                .ToList();

            if (duplicateDrugNames.Any())
            {
                issues.Add(new PrescriptionSafetyIssue
                {
                    RiskLevel = "Medium",
                    ProblematicDrugs = duplicateDrugNames,
                    MedicalReasoning = "The same medication appears more than once in the draft prescription, which can lead to unintended duplicate therapy or excessive total daily exposure.",
                    RelatedPatientRecordOrCondition = "Draft prescription duplicate medication entries",
                    ClinicalRecommendation = "Review duplicate entries and consolidate dose, frequency, and duration before final approval."
                });
            }

            foreach (var item in prescription.Items.Where(i => i.TimesPerDay <= 0 || i.DurationInDays <= 0))
            {
                issues.Add(new PrescriptionSafetyIssue
                {
                    RiskLevel = "High",
                    ProblematicDrugs = new List<string> { item.DrugName ?? $"DrugId {item.DrugId}" },
                    MedicalReasoning = "Frequency and duration must be positive values to support safe administration and clinical interpretation.",
                    RelatedPatientRecordOrCondition = "Draft prescription dosing fields",
                    ClinicalRecommendation = "Correct frequency and duration before final approval."
                });
            }

            return issues;
        }

        private static string NormalizeOverallRisk(PrescriptionSafetyReviewResult result)
        {
            if (!result.HasIssues || !result.Issues.Any())
                return "Low";

            var order = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["Low"] = 1,
                ["Medium"] = 2,
                ["High"] = 3,
                ["Critical"] = 4
            };

            return result.Issues
                .Select(i => order.TryGetValue(i.RiskLevel, out var value) ? (Level: i.RiskLevel, Value: value) : (Level: "Medium", Value: 2))
                .OrderByDescending(i => i.Value)
                .First()
                .Level;
        }

        private static string CleanJsonResponse(string assistantResponse)
        {
            if (assistantResponse.Contains("</thought>"))
                assistantResponse = assistantResponse.Split("</thought>").Last().Trim();

            assistantResponse = assistantResponse.Trim();

            if (assistantResponse.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
                assistantResponse = assistantResponse[7..].Trim();

            if (assistantResponse.StartsWith("```"))
                assistantResponse = assistantResponse[3..].Trim();

            if (assistantResponse.EndsWith("```"))
                assistantResponse = assistantResponse[..^3].Trim();

            return assistantResponse;
        }
    }
}
