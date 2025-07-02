using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ProDialer.Functions.Services;
using ProDialer.Shared.DTOs;
using System.Net;
using System.Text.Json;

namespace ProDialer.Functions.Functions;

/// <summary>
/// Azure Functions for dialing and call management operations
/// Provides endpoints for initiating calls, managing dialing campaigns, and call control
/// </summary>
public class DialingFunctions
{
    private readonly ILogger<DialingFunctions> _logger;
    private readonly DialingEngine _dialingEngine;
    private readonly CommunicationService _communicationService;

    public DialingFunctions(
        ILogger<DialingFunctions> logger,
        DialingEngine dialingEngine,
        CommunicationService communicationService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dialingEngine = dialingEngine ?? throw new ArgumentNullException(nameof(dialingEngine));
        _communicationService = communicationService ?? throw new ArgumentNullException(nameof(communicationService));
    }

    /// <summary>
    /// Timer-triggered function that runs the dialing engine every minute
    /// Processes all active campaigns and initiates appropriate calls
    /// </summary>
    [Function("ProcessDialing")]
    public async Task ProcessDialing([TimerTrigger("0 */1 * * * *")] object timer)
    {
        _logger.LogInformation("Dialing engine timer triggered at {Timestamp}", DateTime.UtcNow);

        try
        {
            var result = await _dialingEngine.ProcessDialingAsync();

            if (result.Success)
            {
                _logger.LogInformation("Dialing engine completed successfully: {CallsInitiated} calls initiated, {LeadsProcessed} leads processed",
                    result.TotalCallsInitiated, result.TotalLeadsProcessed);
            }
            else
            {
                _logger.LogError("Dialing engine failed: {ErrorMessage}", result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in dialing engine timer function");
        }
    }

    /// <summary>
    /// HTTP endpoint to manually trigger dialing engine for a specific campaign
    /// POST /api/dialing/campaigns/{campaignId}/process
    /// </summary>
    [Function("ProcessCampaignDialing")]
    public async Task<HttpResponseData> ProcessCampaignDialing(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "dialing/campaigns/{campaignId}/process")] HttpRequestData req,
        int campaignId)
    {
        _logger.LogInformation("Manual campaign dialing triggered for campaign {CampaignId}", campaignId);

        try
        {
            // Get campaign and validate it exists
            var campaign = await _dialingEngine.GetCampaignAsync(campaignId);
            if (campaign == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"Campaign {campaignId} not found");
                return notFoundResponse;
            }

            var result = await _dialingEngine.ProcessCampaignDialingAsync(campaign);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                Success = result.Success,
                CampaignId = result.CampaignId,
                CampaignName = result.CampaignName,
                CallsInitiated = result.CallsInitiated,
                LeadsProcessed = result.LeadsProcessed,
                ProcessedAt = result.ProcessedAt,
                ErrorMessage = result.ErrorMessage
            });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing campaign {CampaignId} dialing", campaignId);
            
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error processing campaign dialing: {ex.Message}");
            return errorResponse;
        }
    }

    /// <summary>
    /// HTTP endpoint to initiate a manual call for a specific lead
    /// POST /api/dialing/leads/{leadId}/call
    /// </summary>
    [Function("InitiateManualCall")]
    public async Task<HttpResponseData> InitiateManualCall(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "dialing/leads/{leadId}/call")] HttpRequestData req,
        int leadId)
    {
        _logger.LogInformation("Manual call initiation for lead {LeadId}", leadId);

        try
        {
            var requestBody = await req.ReadAsStringAsync();
            var callRequest = JsonSerializer.Deserialize<ManualCallRequestDto>(requestBody ?? "{}");

            if (callRequest == null || string.IsNullOrEmpty(callRequest.AgentId))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Agent ID is required for manual calls");
                return badRequest;
            }

            var result = await _dialingEngine.InitiateManualCallAsync(leadId, callRequest.AgentId, callRequest.PhoneNumber);

            var response = req.CreateResponse(result.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
            await response.WriteAsJsonAsync(new
            {
                Success = result.Success,
                CallId = result.CallId,
                LeadId = leadId,
                AgentId = callRequest.AgentId,
                ErrorMessage = result.ErrorMessage,
                InitiatedAt = result.InitiatedAt
            });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating manual call for lead {LeadId}", leadId);
            
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error initiating call: {ex.Message}");
            return errorResponse;
        }
    }

    /// <summary>
    /// HTTP endpoint to hang up a call
    /// POST /api/dialing/calls/{callId}/hangup
    /// </summary>
    [Function("HangupCall")]
    public async Task<HttpResponseData> HangupCall(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "dialing/calls/{callId}/hangup")] HttpRequestData req,
        string callId)
    {
        _logger.LogInformation("Hangup requested for call {CallId}", callId);

        try
        {
            var requestBody = await req.ReadAsStringAsync();
            var hangupRequest = JsonSerializer.Deserialize<HangupCallRequestDto>(requestBody ?? "{}");

            var result = await _communicationService.HangupCallAsync(callId, hangupRequest?.Reason ?? "Agent hangup");

            var response = req.CreateResponse(result ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
            await response.WriteAsJsonAsync(new
            {
                Success = result,
                CallId = callId,
                HangupAt = DateTime.UtcNow
            });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error hanging up call {CallId}", callId);
            
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error hanging up call: {ex.Message}");
            return errorResponse;
        }
    }

    /// <summary>
    /// HTTP endpoint to transfer a call
    /// POST /api/dialing/calls/{callId}/transfer
    /// </summary>
    [Function("TransferCall")]
    public async Task<HttpResponseData> TransferCall(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "dialing/calls/{callId}/transfer")] HttpRequestData req,
        string callId)
    {
        _logger.LogInformation("Transfer requested for call {CallId}", callId);

        try
        {
            var requestBody = await req.ReadAsStringAsync();
            var transferRequest = JsonSerializer.Deserialize<TransferCallRequestDto>(requestBody ?? "{}");

            if (transferRequest == null || string.IsNullOrEmpty(transferRequest.TargetPhoneNumber))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Target phone number is required for transfer");
                return badRequest;
            }

            var result = await _communicationService.TransferCallAsync(callId, transferRequest.TargetPhoneNumber);

            var response = req.CreateResponse(result.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
            await response.WriteAsJsonAsync(new
            {
                Success = result.Success,
                CallId = callId,
                TargetPhoneNumber = transferRequest.TargetPhoneNumber,
                ErrorMessage = result.ErrorMessage,
                TransferredAt = DateTime.UtcNow
            });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transferring call {CallId}", callId);
            
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error transferring call: {ex.Message}");
            return errorResponse;
        }
    }

    /// <summary>
    /// HTTP endpoint to get dialing engine statistics
    /// GET /api/dialing/statistics
    /// </summary>
    [Function("GetDialingStatistics")]
    public async Task<HttpResponseData> GetDialingStatistics(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "dialing/statistics")] HttpRequestData req)
    {
        _logger.LogInformation("Getting dialing statistics");

        try
        {
            var stats = await _dialingEngine.GetDialingStatisticsAsync();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(stats);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dialing statistics");
            
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error getting statistics: {ex.Message}");
            return errorResponse;
        }
    }

    /// <summary>
    /// HTTP endpoint to control campaign dialing (start/stop/pause)
    /// POST /api/dialing/campaigns/{campaignId}/control
    /// </summary>
    [Function("ControlCampaignDialing")]
    public async Task<HttpResponseData> ControlCampaignDialing(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "dialing/campaigns/{campaignId}/control")] HttpRequestData req,
        int campaignId)
    {
        _logger.LogInformation("Dialing control requested for campaign {CampaignId}", campaignId);

        try
        {
            var requestBody = await req.ReadAsStringAsync();
            var controlRequest = JsonSerializer.Deserialize<CampaignDialingControlDto>(requestBody ?? "{}");

            if (controlRequest == null || string.IsNullOrEmpty(controlRequest.Action))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Action is required (START, STOP, PAUSE, RESUME)");
                return badRequest;
            }

            var result = await _dialingEngine.ControlCampaignDialingAsync(campaignId, controlRequest.Action);

            var response = req.CreateResponse(result.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
            await response.WriteAsJsonAsync(new
            {
                Success = result.Success,
                CampaignId = campaignId,
                Action = controlRequest.Action,
                Message = result.Message,
                ActionedAt = DateTime.UtcNow
            });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error controlling campaign {CampaignId} dialing", campaignId);
            
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error controlling campaign dialing: {ex.Message}");
            return errorResponse;
        }
    }

    /// <summary>
    /// Webhook endpoint for Azure Communication Services call events
    /// POST /api/dialing/webhooks/call-events
    /// </summary>
    [Function("HandleCallEvents")]
    public async Task<HttpResponseData> HandleCallEvents(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "dialing/webhooks/call-events")] HttpRequestData req)
    {
        _logger.LogInformation("Call event webhook received");

        try
        {
            var requestBody = await req.ReadAsStringAsync();
            _logger.LogDebug("Call event payload: {Payload}", requestBody);

            var callEvent = JsonSerializer.Deserialize<CallEventDto>(requestBody ?? "{}");

            if (callEvent != null && !string.IsNullOrEmpty(callEvent.CallId))
            {
                await _dialingEngine.ProcessCallEventAsync(callEvent);
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("Event processed");

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing call event webhook");
            
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error processing call event: {ex.Message}");
            return errorResponse;
        }
    }

    /// <summary>
    /// Get active calls for monitoring
    /// GET /api/dialing/active-calls
    /// </summary>
    [Function("GetActiveCalls")]
    public async Task<HttpResponseData> GetActiveCalls(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "dialing/active-calls")] HttpRequestData req)
    {
        try
        {
            var activeCalls = await _dialingEngine.GetActiveCallsAsync();
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(activeCalls);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active calls");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error getting active calls: {ex.Message}");
            return errorResponse;
        }
    }

    /// <summary>
    /// Start all active campaigns
    /// POST /api/dialing/campaigns/start-all
    /// </summary>
    [Function("StartAllCampaigns")]
    public async Task<HttpResponseData> StartAllCampaigns(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "dialing/campaigns/start-all")] HttpRequestData req)
    {
        try
        {
            var result = await _dialingEngine.StartAllCampaignsAsync();
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting all campaigns");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error starting campaigns: {ex.Message}");
            return errorResponse;
        }
    }

    /// <summary>
    /// Pause all active campaigns
    /// POST /api/dialing/campaigns/pause-all
    /// </summary>
    [Function("PauseAllCampaigns")]
    public async Task<HttpResponseData> PauseAllCampaigns(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "dialing/campaigns/pause-all")] HttpRequestData req)
    {
        try
        {
            var result = await _dialingEngine.PauseAllCampaignsAsync();
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing all campaigns");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error pausing campaigns: {ex.Message}");
            return errorResponse;
        }
    }

    /// <summary>
    /// Stop all active campaigns
    /// POST /api/dialing/campaigns/stop-all
    /// </summary>
    [Function("StopAllCampaigns")]
    public async Task<HttpResponseData> StopAllCampaigns(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "dialing/campaigns/stop-all")] HttpRequestData req)
    {
        try
        {
            var result = await _dialingEngine.StopAllCampaignsAsync();
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping all campaigns");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error stopping campaigns: {ex.Message}");
            return errorResponse;
        }
    }

    /// <summary>
    /// Start a specific campaign
    /// POST /api/dialing/campaigns/{campaignId}/start
    /// </summary>
    [Function("StartCampaign")]
    public async Task<HttpResponseData> StartCampaign(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "dialing/campaigns/{campaignId}/start")] HttpRequestData req,
        int campaignId)
    {
        try
        {
            var result = await _dialingEngine.StartCampaignAsync(campaignId);
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting campaign {CampaignId}", campaignId);
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error starting campaign: {ex.Message}");
            return errorResponse;
        }
    }

    /// <summary>
    /// Pause a specific campaign
    /// POST /api/dialing/campaigns/{campaignId}/pause
    /// </summary>
    [Function("PauseCampaign")]
    public async Task<HttpResponseData> PauseCampaign(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "dialing/campaigns/{campaignId}/pause")] HttpRequestData req,
        int campaignId)
    {
        try
        {
            var result = await _dialingEngine.PauseCampaignAsync(campaignId);
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing campaign {CampaignId}", campaignId);
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error pausing campaign: {ex.Message}");
            return errorResponse;
        }
    }

    /// <summary>
    /// Stop a specific campaign
    /// POST /api/dialing/campaigns/{campaignId}/stop
    /// </summary>
    [Function("StopCampaign")]
    public async Task<HttpResponseData> StopCampaign(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "dialing/campaigns/{campaignId}/stop")] HttpRequestData req,
        int campaignId)
    {
        try
        {
            var result = await _dialingEngine.StopCampaignAsync(campaignId);
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping campaign {CampaignId}", campaignId);
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error stopping campaign: {ex.Message}");
            return errorResponse;
        }
    }
}

/// <summary>
/// DTO for manual call requests
/// </summary>
public class ManualCallRequestDto
{
    public string AgentId { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}

/// <summary>
/// DTO for hangup requests
/// </summary>
public class HangupCallRequestDto
{
    public string? Reason { get; set; }
}

/// <summary>
/// DTO for call transfer requests
/// </summary>
public class TransferCallRequestDto
{
    public string TargetPhoneNumber { get; set; } = string.Empty;
}

/// <summary>
/// DTO for campaign dialing control
/// </summary>
public class CampaignDialingControlDto
{
    public string Action { get; set; } = string.Empty; // START, STOP, PAUSE, RESUME
}

/// <summary>
/// DTO for call events from Azure Communication Services
/// </summary>
