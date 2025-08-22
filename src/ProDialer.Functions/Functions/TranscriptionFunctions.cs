using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using ProDialer.Shared.DTOs;
using ProDialer.Functions.Services;

namespace ProDialer.Functions.Functions;

/// <summary>
/// Azure Functions for managing live transcription functionality
/// Provides endpoints for starting, stopping, and monitoring call transcriptions
/// </summary>
public class TranscriptionFunctions
{
    private readonly ILogger<TranscriptionFunctions> _logger;
    private readonly TranscriptionService _transcriptionService;
    private readonly CommunicationService _communicationService;

    public TranscriptionFunctions(
        ILogger<TranscriptionFunctions> logger,
        TranscriptionService transcriptionService,
        CommunicationService communicationService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _transcriptionService = transcriptionService ?? throw new ArgumentNullException(nameof(transcriptionService));
        _communicationService = communicationService ?? throw new ArgumentNullException(nameof(communicationService));
    }

    /// <summary>
    /// Starts live transcription for a call
    /// </summary>
    [Function("StartTranscription")]
    public async Task<HttpResponseData> StartTranscription(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "transcription/start")] 
        HttpRequestData req,
        FunctionContext context)
    {
        _logger.LogInformation("Starting transcription request received");

        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonSerializer.Deserialize<StartTranscriptionRequest>(requestBody, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (request == null || string.IsNullOrEmpty(request.CallId))
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(new { error = "Invalid request. CallId is required." });
                return badRequestResponse;
            }

            _logger.LogInformation("Starting transcription for call {CallId}", request.CallId);

            var sessionId = await _communicationService.StartTranscriptionAsync(
                request.CallId, 
                request.Language);

            if (sessionId != null)
            {
                var sessionDto = new TranscriptionSessionDto
                {
                    SessionId = sessionId,
                    CallId = request.CallId,
                    IsActive = true,
                    StartedAt = DateTime.UtcNow,
                    Language = request.Language,
                    Status = "Active"
                };

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(sessionDto);
                return response;
            }
            else
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new { error = "Failed to start transcription session" });
                return errorResponse;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting transcription");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { error = ex.Message });
            return errorResponse;
        }
    }

    /// <summary>
    /// Stops live transcription and returns final results
    /// </summary>
    [Function("StopTranscription")]
    public async Task<HttpResponseData> StopTranscription(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "transcription/{sessionId}/stop")] 
        HttpRequestData req,
        string sessionId,
        FunctionContext context)
    {
        _logger.LogInformation("Stopping transcription for session {SessionId}", sessionId);

        try
        {
            var result = await _communicationService.StopTranscriptionAsync(sessionId);

            if (result != null)
            {
                var resultDto = new TranscriptionResultDto
                {
                    SessionId = sessionId,
                    Success = result.Success,
                    Text = result.Text,
                    Language = result.Language,
                    Duration = result.Duration,
                    Confidence = result.Confidence,
                    ErrorMessage = result.ErrorMessage,
                    ProcessedAt = result.ProcessedAt
                };

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(resultDto);
                return response;
            }
            else
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new { error = "Failed to stop transcription session" });
                return errorResponse;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping transcription for session {SessionId}", sessionId);
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { error = ex.Message });
            return errorResponse;
        }
    }

    /// <summary>
    /// Processes uploaded audio for transcription
    /// </summary>
    [Function("TranscribeAudio")]
    public async Task<HttpResponseData> TranscribeAudio(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "transcription/audio")] 
        HttpRequestData req,
        FunctionContext context)
    {
        _logger.LogInformation("Audio transcription request received");

        try
        {
            // Read audio data from request body
            var audioData = new byte[req.Body.Length];
            await req.Body.ReadAsync(audioData, 0, audioData.Length);

            if (audioData.Length == 0)
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(new { error = "No audio data provided" });
                return badRequestResponse;
            }

            // Get language hint from query parameters
            var language = req.Query["language"];

            _logger.LogInformation("Transcribing audio file of {Size} bytes", audioData.Length);

            var result = await _transcriptionService.TranscribeAudioAsync(audioData, language);

            var resultDto = new TranscriptionResultDto
            {
                Success = result.Success,
                Text = result.Text,
                Language = result.Language,
                Duration = result.Duration,
                Confidence = result.Confidence,
                ErrorMessage = result.ErrorMessage,
                ProcessedAt = result.ProcessedAt
            };

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(resultDto);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transcribing audio");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { error = ex.Message });
            return errorResponse;
        }
    }

    /// <summary>
    /// Processes real-time audio chunks for live transcription
    /// </summary>
    [Function("ProcessAudioChunk")]
    public async Task<HttpResponseData> ProcessAudioChunk(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "transcription/{sessionId}/chunk")] 
        HttpRequestData req,
        string sessionId,
        FunctionContext context)
    {
        try
        {
            // Read audio chunk data
            var audioChunk = new byte[req.Body.Length];
            await req.Body.ReadAsync(audioChunk, 0, audioChunk.Length);

            if (audioChunk.Length == 0)
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(new { error = "No audio data provided" });
                return badRequestResponse;
            }

            var result = await _transcriptionService.ProcessAudioChunkAsync(sessionId, audioChunk);

            var updateDto = new LiveTranscriptionUpdateDto
            {
                SessionId = result.SessionId,
                ChunkId = result.ChunkId,
                PartialText = result.PartialText,
                Confidence = result.Confidence,
                IsComplete = result.IsComplete,
                Timestamp = result.ProcessedAt,
                ErrorMessage = result.ErrorMessage
            };

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(updateDto);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing audio chunk for session {SessionId}", sessionId);
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { error = ex.Message });
            return errorResponse;
        }
    }

    /// <summary>
    /// Gets transcription service settings
    /// </summary>
    [Function("GetTranscriptionSettings")]
    public async Task<HttpResponseData> GetTranscriptionSettings(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "transcription/settings")] 
        HttpRequestData req,
        FunctionContext context)
    {
        try
        {
            var settings = new TranscriptionSettingsDto
            {
                EnableByDefault = true,
                DefaultLanguage = "en",
                RealTimeProcessing = true,
                MinConfidenceThreshold = 0.7,
                MaxTextLength = 10000
            };

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(settings);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transcription settings");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { error = ex.Message });
            return errorResponse;
        }
    }
}