using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ProDialer.Functions.Data;
using ProDialer.Functions.Services;
using ProDialer.Shared.Models;
using ProDialer.Shared.DTOs;
using System.Text.Json;

namespace ProDialer.Functions.Services;

/// <summary>
/// Core dialing engine responsible for managing outbound call campaigns
/// Implements VICIdial-style predictive and preview dialing algorithms
/// </summary>
public class DialingEngine
{
    private readonly ILogger<DialingEngine> _logger;
    private readonly ProDialerDbContext _context;
    private readonly CommunicationService _communicationService;
    private readonly LeadFilteringService _leadFilteringService;

    public DialingEngine(
        ILogger<DialingEngine> logger,
        ProDialerDbContext context,
        CommunicationService communicationService,
        LeadFilteringService leadFilteringService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _communicationService = communicationService ?? throw new ArgumentNullException(nameof(communicationService));
        _leadFilteringService = leadFilteringService ?? throw new ArgumentNullException(nameof(leadFilteringService));
    }

    /// <summary>
    /// Main dialing engine that processes all active campaigns
    /// </summary>
    public async Task<DialingEngineResult> ProcessDialingAsync()
    {
        var result = new DialingEngineResult();

        try
        {
            var activeCampaigns = await GetActiveCampaignsAsync();
            _logger.LogInformation("Processing {CampaignCount} active campaigns", activeCampaigns.Count);

            foreach (var campaign in activeCampaigns)
            {
                var campaignResult = await ProcessCampaignDialingAsync(campaign);
                result.CampaignResults.Add(campaignResult);
            }

            result.Success = true;
            result.TotalCallsInitiated = result.CampaignResults.Sum(cr => cr.CallsInitiated);
            result.TotalLeadsProcessed = result.CampaignResults.Sum(cr => cr.LeadsProcessed);

            _logger.LogInformation("Dialing engine completed. {TotalCalls} calls initiated, {TotalLeads} leads processed", 
                result.TotalCallsInitiated, result.TotalLeadsProcessed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in dialing engine processing");
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    /// <summary>
    /// Process dialing for a specific campaign
    /// </summary>
    /// <param name="campaign">Campaign to process</param>
    /// <returns>Campaign dialing results</returns>
    public async Task<CampaignDialingResult> ProcessCampaignDialingAsync(Campaign campaign)
    {
        var result = new CampaignDialingResult { CampaignId = campaign.Id, CampaignName = campaign.Name };

        try
        {
            // Get available agents for this campaign
            var availableAgents = await GetAvailableAgentsAsync(campaign.Id);
            if (!availableAgents.Any())
            {
                _logger.LogInformation("No available agents for campaign {CampaignId}", campaign.Id);
                return result;
            }

            // Calculate dialing ratio and calls needed
            var callsNeeded = CalculateCallsNeeded(campaign, availableAgents.Count);
            if (callsNeeded <= 0)
            {
                _logger.LogInformation("No calls needed for campaign {CampaignId}", campaign.Id);
                return result;
            }

            // Get dialable leads
            var dialableLeads = await GetDialableLeadsAsync(campaign, callsNeeded);
            if (!dialableLeads.Any())
            {
                _logger.LogInformation("No dialable leads found for campaign {CampaignId}", campaign.Id);
                return result;
            }

            _logger.LogInformation("Campaign {CampaignId}: {AgentCount} agents, {CallsNeeded} calls needed, {LeadCount} leads available",
                campaign.Id, availableAgents.Count, callsNeeded, dialableLeads.Count);

            // Process calls based on dial method
            switch (campaign.DialMethod)
            {
                case "PREVIEW":
                    result = await ProcessPreviewDialingAsync(campaign, availableAgents, dialableLeads);
                    break;
                case "PREDICTIVE":
                case "ADAPT_AVERAGE":
                case "ADAPT_HARD_LIMIT":
                case "ADAPT_TAPERED":
                    result = await ProcessPredictiveDialingAsync(campaign, availableAgents, dialableLeads);
                    break;
                case "MANUAL":
                    result = await ProcessManualDialingAsync(campaign, availableAgents, dialableLeads);
                    break;
                default:
                    _logger.LogWarning("Unknown dial method {DialMethod} for campaign {CampaignId}", 
                        campaign.DialMethod, campaign.Id);
                    break;
            }

            result.Success = true;
            _logger.LogInformation("Campaign {CampaignId} processing completed: {CallsInitiated} calls, {LeadsProcessed} leads",
                campaign.Id, result.CallsInitiated, result.LeadsProcessed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing campaign {CampaignId}", campaign.Id);
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    /// <summary>
    /// Process preview dialing (one call per agent, wait for agent action)
    /// </summary>
    private async Task<CampaignDialingResult> ProcessPreviewDialingAsync(Campaign campaign, List<Agent> agents, List<Lead> leads)
    {
        var result = new CampaignDialingResult { CampaignId = campaign.Id, CampaignName = campaign.Name };

        for (int i = 0; i < Math.Min(agents.Count, leads.Count); i++)
        {
            var agent = agents[i];
            var lead = leads[i];

            // Validate lead before dialing
            if (!ValidateLeadForDialing(lead, campaign))
            {
                continue;
            }

            // Create call log entry
            var callLog = await CreateCallLogAsync(campaign, lead, agent);

            // Assign lead to agent for preview
            await AssignLeadToAgentAsync(lead, agent, "PREVIEW");

            result.CallsInitiated++;
            result.LeadsProcessed++;

            _logger.LogDebug("Preview call assigned: Campaign {CampaignId}, Lead {LeadId}, Agent {AgentId}",
                campaign.Id, lead.Id, agent.Id);
        }

        return result;
    }

    /// <summary>
    /// Process predictive dialing (multiple calls per agent based on ratios)
    /// </summary>
    private async Task<CampaignDialingResult> ProcessPredictiveDialingAsync(Campaign campaign, List<Agent> agents, List<Lead> leads)
    {
        var result = new CampaignDialingResult { CampaignId = campaign.Id, CampaignName = campaign.Name };

        var callsToMake = CalculateCallsNeeded(campaign, agents.Count);
        var leadsToProcess = Math.Min(callsToMake, leads.Count);

        for (int i = 0; i < leadsToProcess; i++)
        {
            var lead = leads[i];

            // Validate lead before dialing
            if (!ValidateLeadForDialing(lead, campaign))
            {
                continue;
            }

            // Create call log entry
            var callLog = await CreateCallLogAsync(campaign, lead, null); // No specific agent assigned yet

            // Initiate the call
            var callResult = await InitiateOutboundCallAsync(lead, campaign, callLog);
            
            if (callResult.Success)
            {
                result.CallsInitiated++;
                
                // Update lead status
                lead.Status = "CALLED";
                lead.CallAttempts++;
                lead.LastCalledAt = DateTime.UtcNow;
                lead.CalledSinceLastReset = true;
            }

            result.LeadsProcessed++;
        }

        if (result.CallsInitiated > 0)
        {
            await _context.SaveChangesAsync();
        }

        return result;
    }

    /// <summary>
    /// Process manual dialing (agent-initiated calls)
    /// </summary>
    private async Task<CampaignDialingResult> ProcessManualDialingAsync(Campaign campaign, List<Agent> agents, List<Lead> leads)
    {
        var result = new CampaignDialingResult { CampaignId = campaign.Id, CampaignName = campaign.Name };

        // Manual dialing is typically handled by agent UI actions
        // This method prepares leads for manual dialing by agents
        
        var leadsToAssign = Math.Min(agents.Count * 5, leads.Count); // Queue up to 5 leads per agent

        for (int i = 0; i < leadsToAssign; i++)
        {
            var lead = leads[i];

            if (ValidateLeadForDialing(lead, campaign))
            {
                // Mark lead as available for manual dialing
                lead.Status = "QUEUE";
                result.LeadsProcessed++;
            }
        }

        if (result.LeadsProcessed > 0)
        {
            await _context.SaveChangesAsync();
        }

        return result;
    }

    /// <summary>
    /// Validate that a lead is ready for dialing
    /// </summary>
    private bool ValidateLeadForDialing(Lead lead, Campaign campaign)
    {
        // Check if lead is excluded
        if (lead.IsExcluded)
        {
            _logger.LogDebug("Lead {LeadId} is excluded from calling", lead.Id);
            return false;
        }

        // Check if lead has valid phone number
        if (string.IsNullOrEmpty(lead.PrimaryPhone))
        {
            _logger.LogDebug("Lead {LeadId} has no primary phone number", lead.Id);
            return false;
        }

        // Validate phone number format
        if (!ValidationUtilities.IsValidPhoneNumber(lead.PrimaryPhone))
        {
            _logger.LogDebug("Lead {LeadId} has invalid phone number format", lead.Id);
            return false;
        }

        // Check calling time restrictions
        var leadTimezone = ValidationUtilities.GetTimezoneFromAreaCode(lead.PrimaryPhone);
        if (leadTimezone != null && !ValidationUtilities.IsWithinCallingHours(leadTimezone))
        {
            _logger.LogDebug("Lead {LeadId} is outside calling hours for timezone", lead.Id);
            return false;
        }

        // Check if enough time has passed since last call
        if (lead.LastCalledAt.HasValue && campaign.MinCallInterval > 0)
        {
            var timeSinceLastCall = DateTime.UtcNow - lead.LastCalledAt.Value;
            if (timeSinceLastCall.TotalMinutes < campaign.MinCallInterval)
            {
                _logger.LogDebug("Lead {LeadId} called too recently ({MinutesAgo} minutes ago)", 
                    lead.Id, timeSinceLastCall.TotalMinutes);
                return false;
            }
        }

        // Check maximum call attempts
        if (campaign.MaxCallAttempts > 0 && lead.CallAttempts >= campaign.MaxCallAttempts)
        {
            _logger.LogDebug("Lead {LeadId} has reached maximum call attempts ({Attempts})", 
                lead.Id, lead.CallAttempts);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Get campaigns that are currently active and should be processed
    /// </summary>
    private async Task<List<Campaign>> GetActiveCampaignsAsync()
    {
        return await _context.Campaigns
            .Where(c => c.IsActive)
            .Include(c => c.CampaignLists)
                .ThenInclude(cl => cl.List)
            .ToListAsync();
    }

    /// <summary>
    /// Get agents available for dialing in a campaign
    /// </summary>
    private async Task<List<Agent>> GetAvailableAgentsAsync(int campaignId)
    {
        return await _context.Agents
            .Where(a => a.Status == "Available" || a.Status == "Waiting")
            .ToListAsync();
    }

    /// <summary>
    /// Get leads that are ready for dialing in a campaign
    /// </summary>
    private async Task<List<Lead>> GetDialableLeadsAsync(Campaign campaign, int maxLeads)
    {
        var listIds = campaign.CampaignLists.Select(cl => cl.ListId).ToList();

        var baseQuery = _context.Leads
            .Where(l => listIds.Contains(l.ListId) && 
                       !l.IsExcluded &&
                       !string.IsNullOrEmpty(l.PrimaryPhone) &&
                       (l.Status == "NEW" || l.Status == "CALLBACK" || l.Status == "RETRY"));

        // Apply campaign-specific filters
        if (campaign.MaxCallAttempts > 0)
        {
            baseQuery = baseQuery.Where(l => l.CallAttempts < campaign.MaxCallAttempts);
        }

        // Apply time-based filters
        if (campaign.MinCallInterval > 0)
        {
            var minTime = DateTime.UtcNow.AddMinutes(-campaign.MinCallInterval);
            baseQuery = baseQuery.Where(l => !l.LastCalledAt.HasValue || l.LastCalledAt < minTime);
        }

        // Order by priority and creation date
        return await baseQuery
            .OrderByDescending(l => l.Priority)
            .ThenBy(l => l.CreatedAt)
            .Take(maxLeads)
            .ToListAsync();
    }

    /// <summary>
    /// Calculate how many calls are needed based on campaign settings and available agents
    /// </summary>
    private int CalculateCallsNeeded(Campaign campaign, int availableAgents)
    {
        if (availableAgents == 0) return 0;

        switch (campaign.DialMethod)
        {
            case "PREVIEW":
            case "MANUAL":
                // One call per agent for preview/manual
                return availableAgents;

            case "PREDICTIVE":
            case "ADAPT_AVERAGE":
            case "ADAPT_HARD_LIMIT":
            case "ADAPT_TAPERED":
                // Use dialing ratio for predictive
                var ratio = campaign.DialingRatio > 0 ? campaign.DialingRatio : 1.5m;
                return (int)Math.Ceiling(availableAgents * ratio);

            default:
                return availableAgents;
        }
    }

    /// <summary>
    /// Create a call log entry for tracking
    /// </summary>
    private async Task<CallLog> CreateCallLogAsync(Campaign campaign, Lead lead, Agent? agent)
    {
        var callLog = new CallLog
        {
            CampaignId = campaign.Id,
            LeadId = lead.Id,
            AgentId = agent?.Id.ToString(),
            PhoneNumber = lead.PrimaryPhone,
            CallDirection = "Outbound",
            CallStatus = "Initiated",
            StartedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.CallLogs.Add(callLog);
        await _context.SaveChangesAsync();

        return callLog;
    }

    /// <summary>
    /// Initiate an outbound call through Azure Communication Services
    /// </summary>
    private async Task<CallResult> InitiateOutboundCallAsync(Lead lead, Campaign campaign, CallLog callLog)
    {
        try
        {
            var callRequest = new OutboundCallRequest
            {
                ToPhoneNumber = lead.PrimaryPhone,
                FromPhoneNumber = campaign.OutboundCallerId ?? "+18005551234", // Default or campaign-specific
                CampaignId = campaign.Id,
                LeadId = lead.Id,
                CallLogId = callLog.Id
            };

            var result = await _communicationService.InitiateOutboundCallAsync(callRequest);

            // Update call log with result
            callLog.CallId = result.CallId;
            callLog.CallStatus = result.Success ? "Ringing" : "Failed";
            callLog.UpdatedAt = DateTime.UtcNow;

            if (!result.Success)
            {
                callLog.HangupCause = result.ErrorMessage;
            }

            await _context.SaveChangesAsync();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating call for lead {LeadId}", lead.Id);
            
            callLog.CallStatus = "Failed";
            callLog.HangupCause = ex.Message;
            callLog.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new CallResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    /// <summary>
    /// Assign a lead to an agent for preview dialing
    /// </summary>
    private async Task AssignLeadToAgentAsync(Lead lead, Agent agent, string assignmentType)
    {
        lead.User = agent.UserId;
        lead.LastHandlerAgent = $"{agent.FirstName} {agent.LastName}";
        lead.Status = assignmentType;
        lead.UpdatedAt = DateTime.UtcNow;

        agent.Status = "PreviewCall";
        agent.LastLoggedOutAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Assigned lead {LeadId} to agent {AgentId} for {AssignmentType}",
            lead.Id, agent.Id, assignmentType);
    }

    /// <summary>
    /// Get a campaign by ID (used by DialingFunctions)
    /// </summary>
    /// <param name="campaignId">Campaign ID</param>
    /// <returns>Campaign if found, null otherwise</returns>
    public async Task<Campaign?> GetCampaignAsync(int campaignId)
    {
        return await _context.Campaigns
            .Include(c => c.CampaignLists)
                .ThenInclude(cl => cl.List)
            .FirstOrDefaultAsync(c => c.Id == campaignId);
    }

    /// <summary>
    /// Initiate a manual call for a specific lead
    /// </summary>
    /// <param name="leadId">Lead ID</param>
    /// <param name="agentId">Agent ID</param>
    /// <param name="phoneNumber">Optional specific phone number to call</param>
    /// <returns>Call result</returns>
    public async Task<CallResult> InitiateManualCallAsync(int leadId, string agentId, string? phoneNumber = null)
    {
        try
        {
            var lead = await _context.Leads.FindAsync(leadId);
            if (lead == null)
            {
                return new CallResult
                {
                    Success = false,
                    ErrorMessage = $"Lead {leadId} not found"
                };
            }

            var agent = await _context.Agents.FirstOrDefaultAsync(a => a.UserId == agentId);
            if (agent == null)
            {
                return new CallResult
                {
                    Success = false,
                    ErrorMessage = $"Agent {agentId} not found"
                };
            }

            // Use provided phone number or lead's primary phone
            var targetPhone = phoneNumber ?? lead.PrimaryPhone;
            if (string.IsNullOrEmpty(targetPhone))
            {
                return new CallResult
                {
                    Success = false,
                    ErrorMessage = "No phone number available for calling"
                };
            }

            // Validate lead for calling
            var campaign = await _context.Campaigns
                .Include(c => c.CampaignLists)
                .FirstOrDefaultAsync(c => c.CampaignLists.Any(cl => cl.ListId == lead.ListId));

            if (campaign == null)
            {
                return new CallResult
                {
                    Success = false,
                    ErrorMessage = "No active campaign found for this lead"
                };
            }

            if (!ValidateLeadForDialing(lead, campaign))
            {
                return new CallResult
                {
                    Success = false,
                    ErrorMessage = "Lead is not eligible for calling at this time"
                };
            }

            // Create call log
            var callLog = await CreateCallLogAsync(campaign, lead, agent);

            // Initiate the call
            var callRequest = new OutboundCallRequest
            {
                ToPhoneNumber = targetPhone,
                FromPhoneNumber = campaign.OutboundCallerId ?? "+18005551234",
                CampaignId = campaign.Id,
                LeadId = lead.Id,
                CallLogId = callLog.Id
            };

            var result = await _communicationService.InitiateOutboundCallAsync(callRequest);

            // Update lead and agent status
            if (result.Success)
            {
                lead.Status = "CALLED";
                lead.CallAttempts++;
                lead.LastCalledAt = DateTime.UtcNow;
                lead.User = agentId;

                agent.Status = "OnCall";
                agent.LastLoggedOutAt = DateTime.UtcNow;

                callLog.CallId = result.CallId;
                callLog.CallStatus = "Ringing";

                await _context.SaveChangesAsync();
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating manual call for lead {LeadId}", leadId);
            return new CallResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Get dialing engine statistics
    /// </summary>
    /// <returns>Dialing statistics</returns>
    public async Task<DialingStatisticsDto> GetDialingStatisticsAsync()
    {
        var stats = new DialingStatisticsDto();

        try
        {
            // Get active campaigns count
            stats.ActiveCampaigns = await _context.Campaigns
                .CountAsync(c => c.IsActive);

            // Get available agents count
            stats.AvailableAgents = await _context.Agents
                .CountAsync(a => a.Status == "Available");

            // Get agents on calls
            stats.AgentsOnCall = await _context.Agents
                .CountAsync(a => a.Status == "OnCall" || a.Status == "Busy");

            // Get today's call statistics
            var today = DateTime.UtcNow.Date;
            var callLogs = await _context.CallLogs
                .Where(cl => cl.StartedAt >= today)
                .ToListAsync();

            stats.CallsToday = callLogs.Count;
            stats.AnsweredCallsToday = callLogs.Count(cl => cl.CallStatus == "ANSWERED");
            stats.CallsInProgress = callLogs.Count(cl => cl.CallStatus == "RINGING" || cl.CallStatus == "CONNECTED");

            // Calculate answer rate
            if (stats.CallsToday > 0)
            {
                stats.AnswerRateToday = (decimal)stats.AnsweredCallsToday / stats.CallsToday * 100;
            }

            // Get leads ready for calling
            stats.LeadsReadyForCalling = await _context.Leads
                .CountAsync(l => !l.IsExcluded && 
                               !string.IsNullOrEmpty(l.PrimaryPhone) &&
                               (l.Status == "NEW" || l.Status == "CALLBACK"));

            stats.GeneratedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating dialing statistics");
        }

        return stats;
    }

    /// <summary>
    /// Control campaign dialing (start, stop, pause, resume)
    /// </summary>
    /// <param name="campaignId">Campaign ID</param>
    /// <param name="action">Action to take</param>
    /// <returns>Control result</returns>
    public async Task<CampaignControlResult> ControlCampaignDialingAsync(int campaignId, string action)
    {
        try
        {
            var campaign = await _context.Campaigns.FindAsync(campaignId);
            if (campaign == null)
            {
                return new CampaignControlResult
                {
                    Success = false,
                    Message = $"Campaign {campaignId} not found"
                };
            }

            switch (action.ToUpper())
            {
                case "START":
                    campaign.IsActive = true;
                    break;
                case "STOP":
                    campaign.IsActive = false;
                    break;
                case "PAUSE":
                    campaign.IsActive = false;
                    break;
                case "RESUME":
                    campaign.IsActive = true;
                    break;
                default:
                    return new CampaignControlResult
                    {
                        Success = false,
                        Message = $"Invalid action: {action}. Valid actions are START, STOP, PAUSE, RESUME"
                    };
            }

            campaign.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Campaign {CampaignId} dialing {Action} - IsActive: {IsActive}", 
                campaignId, action, campaign.IsActive);

            return new CampaignControlResult
            {
                Success = true,
                Message = $"Campaign dialing {action.ToLower()}ed successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error controlling campaign {CampaignId} dialing with action {Action}", 
                campaignId, action);
            
            return new CampaignControlResult
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    /// <summary>
    /// Process call events from Azure Communication Services webhooks
    /// </summary>
    /// <param name="callEvent">Call event data</param>
    /// <returns>Task</returns>
    public async Task ProcessCallEventAsync(CallEventDto callEvent)
    {
        try
        {
            _logger.LogInformation("Processing call event: {EventType} for call {CallId}", 
                callEvent.EventType, callEvent.CallId);

            // Find the call log associated with this call
            var callLog = await _context.CallLogs
                .FirstOrDefaultAsync(cl => cl.CallId == callEvent.CallId);

            if (callLog == null)
            {
                _logger.LogWarning("Call log not found for call {CallId}", callEvent.CallId);
                return;
            }

            // Update call log based on event type
            switch (callEvent.EventType?.ToUpper())
            {
                case "CALLCONNECTED":
                    callLog.CallStatus = "CONNECTED";
                    callLog.AnsweredAt = callEvent.EventTime;
                    break;

                case "CALLENDED":
                case "CALLDISCONNECTED":
                    callLog.CallStatus = "ENDED";
                    callLog.EndedAt = callEvent.EventTime;
                    callLog.HangupCause = callEvent.HangupReason;
                    
                    if (callEvent.CallDuration.HasValue)
                    {
                        callLog.DurationSeconds = callEvent.CallDuration.Value;
                    }
                    
                    // Update lead status
                    var lead = await _context.Leads.FindAsync(callLog.LeadId);
                    if (lead != null)
                    {
                        lead.LastContactedAt = callEvent.EventTime;
                        
                        // Determine final status based on call duration
                        if (callLog.DurationSeconds > 0)
                        {
                            lead.Status = "CONTACTED";
                        }
                        else
                        {
                            lead.Status = "NO_ANSWER";
                        }
                    }

                    // Update agent status back to available
                    if (!string.IsNullOrEmpty(callLog.AgentId))
                    {
                        var agent = await _context.Agents
                            .FirstOrDefaultAsync(a => a.Id.ToString() == callLog.AgentId);
                        if (agent != null)
                        {
                            agent.Status = "Available";
                            agent.LastLoggedOutAt = DateTime.UtcNow;
                        }
                    }
                    break;

                case "CALLFAILED":
                    callLog.CallStatus = "FAILED";
                    callLog.HangupCause = callEvent.HangupReason ?? "Call failed";
                    break;
            }

            callLog.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogDebug("Call event processed successfully for call {CallId}", callEvent.CallId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing call event for call {CallId}", callEvent.CallId);
        }
    }

    /// <summary>
    /// Get all currently active calls in the system
    /// </summary>
    public async Task<List<ActiveCallDto>> GetActiveCallsAsync()
    {
        try
        {
            var activeCalls = await _context.CallLogs
                .Where(c => c.CallStatus == "RINGING" || c.CallStatus == "ANSWERED" || c.CallStatus == "IN_PROGRESS")
                .Include(c => c.Campaign)
                .Include(c => c.Lead)
                .Select(c => new ActiveCallDto
                {
                    CallId = c.CallId ?? string.Empty,
                    CampaignId = c.CampaignId,
                    CampaignName = c.Campaign.Name,
                    LeadId = c.LeadId,
                    LeadName = $"{c.Lead.FirstName} {c.Lead.LastName}".Trim(),
                    PhoneNumber = c.Lead.PrimaryPhone,
                    AgentUsername = c.AgentId ?? "System",
                    CallStatus = c.CallStatus,
                    CallStartTime = c.StartedAt,
                    CallDurationSeconds = c.DurationSeconds
                })
                .ToListAsync();

            _logger.LogDebug("Retrieved {CallCount} active calls", activeCalls.Count);
            return activeCalls;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active calls");
            return new List<ActiveCallDto>();
        }
    }

    /// <summary>
    /// Start all campaigns in the system
    /// </summary>
    public async Task<CampaignControlResult> StartAllCampaignsAsync()
    {
        try
        {
            var campaigns = await _context.Campaigns.ToListAsync();
            var startedCount = 0;

            foreach (var campaign in campaigns)
            {
                if (!campaign.IsActive)
                {
                    campaign.IsActive = true;
                    campaign.UpdatedAt = DateTime.UtcNow;
                    startedCount++;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Started {Count} campaigns", startedCount);
            return new CampaignControlResult
            {
                Success = true,
                Message = $"Started {startedCount} campaigns successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting all campaigns");
            return new CampaignControlResult
            {
                Success = false,
                Message = $"Error starting campaigns: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Pause all campaigns in the system
    /// </summary>
    public async Task<CampaignControlResult> PauseAllCampaignsAsync()
    {
        try
        {
            var campaigns = await _context.Campaigns.ToListAsync();
            var pausedCount = 0;

            foreach (var campaign in campaigns)
            {
                if (campaign.IsActive)
                {
                    campaign.IsActive = false;
                    campaign.UpdatedAt = DateTime.UtcNow;
                    pausedCount++;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Paused {Count} campaigns", pausedCount);
            return new CampaignControlResult
            {
                Success = true,
                Message = $"Paused {pausedCount} campaigns successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing all campaigns");
            return new CampaignControlResult
            {
                Success = false,
                Message = $"Error pausing campaigns: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Stop all campaigns in the system
    /// </summary>
    public async Task<CampaignControlResult> StopAllCampaignsAsync()
    {
        try
        {
            var campaigns = await _context.Campaigns.ToListAsync();
            var stoppedCount = 0;

            foreach (var campaign in campaigns)
            {
                if (campaign.IsActive)
                {
                    campaign.IsActive = false;
                    campaign.UpdatedAt = DateTime.UtcNow;
                    stoppedCount++;
                }
            }

            await _context.SaveChangesAsync();

            // Also end any active calls for stopped campaigns
            var activeCalls = await _context.CallLogs
                .Where(c => c.CallStatus == "RINGING" || c.CallStatus == "ANSWERED" || c.CallStatus == "IN_PROGRESS")
                .ToListAsync();

            foreach (var call in activeCalls)
            {
                call.CallStatus = "HANGUP";
                call.HangupCause = "Campaign stopped";
                call.EndedAt = DateTime.UtcNow;
                call.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Stopped {Count} campaigns and ended {CallCount} active calls", stoppedCount, activeCalls.Count);
            return new CampaignControlResult
            {
                Success = true,
                Message = $"Stopped {stoppedCount} campaigns and ended {activeCalls.Count} active calls successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping all campaigns");
            return new CampaignControlResult
            {
                Success = false,
                Message = $"Error stopping campaigns: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Start a specific campaign
    /// </summary>
    public async Task<CampaignControlResult> StartCampaignAsync(int campaignId)
    {
        try
        {
            var campaign = await _context.Campaigns.FindAsync(campaignId);
            if (campaign == null)
            {
                return new CampaignControlResult
                {
                    Success = false,
                    Message = $"Campaign {campaignId} not found"
                };
            }

            if (campaign.IsActive)
            {
                return new CampaignControlResult
                {
                    Success = true,
                    Message = $"Campaign '{campaign.Name}' is already active"
                };
            }

            campaign.IsActive = true;
            campaign.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Started campaign {CampaignId}: {CampaignName}", campaignId, campaign.Name);
            return new CampaignControlResult
            {
                Success = true,
                Message = $"Campaign '{campaign.Name}' started successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting campaign {CampaignId}", campaignId);
            return new CampaignControlResult
            {
                Success = false,
                Message = $"Error starting campaign: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Pause a specific campaign
    /// </summary>
    public async Task<CampaignControlResult> PauseCampaignAsync(int campaignId)
    {
        try
        {
            var campaign = await _context.Campaigns.FindAsync(campaignId);
            if (campaign == null)
            {
                return new CampaignControlResult
                {
                    Success = false,
                    Message = $"Campaign {campaignId} not found"
                };
            }

            if (!campaign.IsActive)
            {
                return new CampaignControlResult
                {
                    Success = true,
                    Message = $"Campaign '{campaign.Name}' is already paused"
                };
            }

            campaign.IsActive = false;
            campaign.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Paused campaign {CampaignId}: {CampaignName}", campaignId, campaign.Name);
            return new CampaignControlResult
            {
                Success = true,
                Message = $"Campaign '{campaign.Name}' paused successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing campaign {CampaignId}", campaignId);
            return new CampaignControlResult
            {
                Success = false,
                Message = $"Error pausing campaign: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Stop a specific campaign
    /// </summary>
    public async Task<CampaignControlResult> StopCampaignAsync(int campaignId)
    {
        try
        {
            var campaign = await _context.Campaigns.FindAsync(campaignId);
            if (campaign == null)
            {
                return new CampaignControlResult
                {
                    Success = false,
                    Message = $"Campaign {campaignId} not found"
                };
            }

            if (!campaign.IsActive)
            {
                return new CampaignControlResult
                {
                    Success = true,
                    Message = $"Campaign '{campaign.Name}' is already stopped"
                };
            }

            campaign.IsActive = false;
            campaign.UpdatedAt = DateTime.UtcNow;

            // Also end any active calls for this campaign
            var activeCalls = await _context.CallLogs
                .Where(c => c.CampaignId == campaignId && 
                          (c.CallStatus == "RINGING" || c.CallStatus == "ANSWERED" || c.CallStatus == "IN_PROGRESS"))
                .ToListAsync();

            foreach (var call in activeCalls)
            {
                call.CallStatus = "HANGUP";
                call.HangupCause = "Campaign stopped";
                call.EndedAt = DateTime.UtcNow;
                call.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Stopped campaign {CampaignId}: {CampaignName} and ended {CallCount} active calls", 
                campaignId, campaign.Name, activeCalls.Count);
            
            return new CampaignControlResult
            {
                Success = true,
                Message = $"Campaign '{campaign.Name}' stopped successfully. Ended {activeCalls.Count} active calls."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping campaign {CampaignId}", campaignId);
            return new CampaignControlResult
            {
                Success = false,
                Message = $"Error stopping campaign: {ex.Message}"
            };
        }
    }
}

/// <summary>
/// Result of the dialing engine processing
/// </summary>
public class DialingEngineResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int TotalCallsInitiated { get; set; }
    public int TotalLeadsProcessed { get; set; }
    public List<CampaignDialingResult> CampaignResults { get; set; } = new();
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Result of processing a single campaign
/// </summary>
public class CampaignDialingResult
{
    public int CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int CallsInitiated { get; set; }
    public int LeadsProcessed { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Request for initiating an outbound call
/// </summary>
public class OutboundCallRequest
{
    public string ToPhoneNumber { get; set; } = string.Empty;
    public string FromPhoneNumber { get; set; } = string.Empty;
    public int CampaignId { get; set; }
    public int LeadId { get; set; }
    public int CallLogId { get; set; }
}

/// <summary>
/// Result of a call initiation attempt
/// </summary>
public class CallResult
{
    public bool Success { get; set; }
    public string? CallId { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime InitiatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Data transfer object for active calls
/// </summary>
public class ActiveCallDto
{
    public string CallId { get; set; } = string.Empty;
    public int CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public int LeadId { get; set; }
    public string LeadName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string AgentUsername { get; set; } = string.Empty;
    public string CallStatus { get; set; } = string.Empty;
    public DateTime CallStartTime { get; set; } = DateTime.UtcNow;
    public int CallDurationSeconds { get; set; } = 0;
}
