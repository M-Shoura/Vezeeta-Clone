using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using Application.Interfaces.Services;
using Application.Interfaces.Repositories;
using Domain.Entities;

namespace Presentation.Hubs
{
    public class ConsultationHub : Hub
    {
        // Thread-safe dictionary tracking distinct binary stream buffers mapped by Appointment ID
        private static readonly ConcurrentDictionary<string, MemoryStream> AudioBuffers = new();
        private readonly IMedicalAiService _aiService;
        /// <summary>
        /// Instantiates the hub with the registered Groq-powered medical AI handler service.
        /// </summary>
        public ConsultationHub(IMedicalAiService aiService)
        {
            _aiService = aiService;


        }

        /// <summary>
        /// Accrues oncoming sliced audio byte arrays arriving over the live JavaScript streaming connection.
        /// </summary>
        public async Task SendAudioChunk(string appointmentId, List<byte> chunk)
        {
            if (string.IsNullOrWhiteSpace(appointmentId) || chunk == null || chunk.Count == 0)
            {
                return;
            }


            // Retrieve or initialize a memory stream scope specifically bound to this unique appointment tracking ID
            var memoryStream = AudioBuffers.GetOrAdd(appointmentId, _ => new MemoryStream());

            // Lock block guarantees synchronous sequential append processing for arriving packet clusters
            lock (memoryStream)
            {
                byte[] bytes = chunk.ToArray();
                memoryStream.Write(bytes, 0, bytes.Length);
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Flushes audio collection data, executes Whisper transcription, routes the clinical context through the LLM, 
        /// and fires the structured results back down to the doctor's page.
        /// </summary>
        public async Task StopAndAnalyze(string appointmentId)
        {
            if (string.IsNullOrWhiteSpace(appointmentId)) return;

            // 1. Extract and isolate memory references to purge the global tracking index
            if (AudioBuffers.TryRemove(appointmentId, out var finalAudioStream))
            {
                try
                {
                    // Give a 300ms window for any lingering asynchronous chunks to finish processing
                    await Task.Delay(300);

                    byte[] audioBytes;
                    lock (finalAudioStream)
                    {
                        audioBytes = finalAudioStream.ToArray();
                    }

                    if (audioBytes.Length < 1000) // If less than ~1KB, it's an empty artifact
                    {
                        await Clients.Caller.SendAsync("ReceiveAnalysis", "خطأ", "التسجيل قصير جداً أو فارغ.");
                        return;
                    }

                    // Create a completely clean isolated memory container
                    using var cleanStream = new MemoryStream(audioBytes);

                    // Send to your AI Service
                    string transcript = await _aiService.TranscribeAudioAsync(cleanStream);

                    if (string.IsNullOrWhiteSpace(transcript) || transcript.StartsWith("["))
                    {
                        await Clients.Caller.SendAsync("ReceiveAnalysis", "لم يتم التقاط الصوت", "يرجى إعادة المحاولة.");
                        return;
                    }

                    var aiResult = await _aiService.AnalyzeConsultationAsync(transcript,appointmentId);
                    string structuredSymptoms = string.Join(", ", aiResult.Symptoms);

                    await Clients.Caller.SendAsync("ReceiveAnalysis", structuredSymptoms, aiResult.DiagnosticSuggestions);
                }
                catch (Exception ex)
                {
                    await Clients.Caller.SendAsync("ReceiveAnalysis", "Error", ex.Message);
                }
                finally
                {
                    await finalAudioStream.DisposeAsync();
                }
            }
            else
            {
                await Clients.Caller.SendAsync("ReceiveAnalysis", "Error", "No audio tracking container found.");
            }
        }
    }
}