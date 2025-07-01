using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ProDialer.Functions.Services;
using ProDialer.Functions.Data;
using ProDialer.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ProDialer.Functions.Functions;

/// <summary>
/// Azure Functions for background processing and campaign automation
/// Handles scheduled tasks, campaign processing, and maintenance operations
/// Following Azure best practices with minimal service layer and direct business logic
/// </summary>
public class BackgroundProcessingFunctions
{
    private readonly ILogger<BackgroundProcessingFunctions> _logger;
    private readonly ProDialerDbContext _context;
    private readonly TableStorageService _tableStorageService;
    private readonly CommunicationService _communicationService;

    public BackgroundProcessingFunctions(
        ILogger<BackgroundProcessingFunctions> logger,
        ProDialerDbContext context,
        TableStorageService tableStorageService,
        CommunicationService communicationService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _tableStorageService = tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
        _communicationService = communicationService ?? throw new ArgumentNullException(nameof(communicationService));
    }

    /// <summary>
    /// Processes active campaigns for outbound dialing
    /// Runs every minute during business hours
    /// </summary>
    [Function("ProcessActiveCampaigns")]
    public async Task ProcessActiveCampaigns(
        [TimerTrigger("0 * * * * *")] object timer) // Every minute
    {
        try
        {
            _logger.LogInformation("Starting active campaign processing at {Time}", DateTime.UtcNow);

            // Get all active campaigns that should be processed
            var activeCampaigns = await GetActiveCampaignsForProcessingAsync();

            if (!activeCampaigns.Any())
            {
                _logger.LogInformation("No active campaigns found for processing");
                return;
            }

            _logger.LogInformation("Found {CampaignCount} active campaigns to process", activeCampaigns.Count);

            var processedCount = 0;
            var errors = new List<string>();

            // Process each campaign
            foreach (var campaignId in activeCampaigns)
            {
                try
                {
                    var result = await ProcessCampaignAsync(campaignId);
                    
                    if (result.Success)
                    {
                        processedCount++;
                        _logger.LogInformation(
                            "Campaign {CampaignId} processed successfully: {Successful} calls initiated, {Failed} failed",
                            campaignId, result.SuccessfulCalls, result.FailedCalls);
                    }
                    else
                    {
                        errors.Add($"Campaign {campaignId}: {result.ErrorMessage}");
                        _logger.LogWarning("Campaign {CampaignId} processing failed: {Error}", 
                            campaignId, result.ErrorMessage);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Campaign {campaignId}: {ex.Message}");
                    _logger.LogError(ex, "Error processing campaign {CampaignId}", campaignId);
                }
            }

            _logger.LogInformation(
                "Campaign processing completed: {ProcessedCount} successful, {ErrorCount} errors",
                processedCount, errors.Count);

            if (errors.Any())
            {
                _logger.LogWarning("Campaign processing errors: {Errors}", string.Join("; ", errors));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error in campaign processing timer function");
        }
    }

    /// <summary>
    /// Performs DNC scrubbing for all active campaigns
    /// Runs every hour to ensure compliance
    /// </summary>
    [Function("DncAutoScrub")]
    public async Task DncAutoScrub(
        [TimerTrigger("0 0 * * * *")] object timer, // Every hour
        FunctionContext context)
    {
        try
        {
            _logger.LogInformation("Starting automatic DNC scrubbing at {Time}", DateTime.UtcNow);

            var activeCampaigns = await GetActiveCampaignsForProcessingAsync();

            if (!activeCampaigns.Any())
            {
                _logger.LogInformation("No active campaigns found for DNC scrubbing");
                return;
            }

            var scrubResults = new List<string>();

            foreach (var campaignId in activeCampaigns)
            {
                try
                {
                    var result = await PerformDncScrubAsync(campaignId);
                    
                    if (result.Success)
                    {
                        scrubResults.Add($"Campaign {campaignId}: {result.DncMatchesFound} DNC matches, {result.LeadsUpdated} leads updated");
                        _logger.LogInformation(
                            "DNC scrubbing completed for campaign {CampaignId}: {Processed} processed, {Matches} matches, {Updated} updated",
                            campaignId, result.TotalLeadsProcessed, result.DncMatchesFound, result.LeadsUpdated);
                    }
                    else
                    {
                        _logger.LogWarning("DNC scrubbing failed for campaign {CampaignId}: {Error}", 
                            campaignId, result.ErrorMessage);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during DNC scrubbing for campaign {CampaignId}", campaignId);
                }
            }

            _logger.LogInformation("DNC auto-scrub completed for {CampaignCount} campaigns", activeCampaigns.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error in DNC auto-scrub timer function");
        }
    }

    /// <summary>
    /// Validates and updates timezone information for leads
    /// Runs daily to ensure accurate timezone data
    /// </summary>
    [Function("TimezoneValidation")]
    public async Task TimezoneValidation(
        [TimerTrigger("0 0 2 * * *")] object timer, // Daily at 2 AM
        FunctionContext context)
    {
        try
        {
            _logger.LogInformation("Starting timezone validation at {Time}", DateTime.UtcNow);

            // Get leads that need timezone validation (placeholder implementation)
            var campaignIds = await GetActiveCampaignsForProcessingAsync();
            _logger.LogInformation("Processing timezone validation for {CampaignCount} campaigns", campaignIds.Count);

            // This would be implemented to validate and update timezone information
            // For now, we'll just log that it ran
            await Task.Delay(100); // Add actual async operation
            _logger.LogInformation("Timezone validation completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in timezone validation timer function");
        }
    }

    /// <summary>
    /// Performs phone number validation for newly imported leads
    /// Runs every 30 minutes to validate new numbers
    /// </summary>
    [Function("PhoneValidation")]
    public async Task PhoneValidation(
        [TimerTrigger("0 */30 * * * *")] object timer, // Every 30 minutes
        FunctionContext context)
    {
        try
        {
            _logger.LogInformation("Starting phone validation at {Time}", DateTime.UtcNow);

            // Get leads that need phone validation (placeholder implementation)
            var campaignIds = await GetActiveCampaignsForProcessingAsync();
            _logger.LogInformation("Processing phone validation for {CampaignCount} campaigns", campaignIds.Count);

            // This would be implemented to validate phone numbers for new leads
            // For now, we'll just log that it ran
            await Task.Delay(100); // Add actual async operation
            _logger.LogInformation("Phone validation completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in phone validation timer function");
        }
    }

    /// <summary>
    /// Cleans up old data and performs maintenance tasks
    /// Runs daily to maintain system performance
    /// </summary>
    [Function("MaintenanceTasks")]
    public async Task MaintenanceTasks(
        [TimerTrigger("0 0 3 * * *")] object timer, // Daily at 3 AM
        FunctionContext context)
    {
        try
        {
            _logger.LogInformation("Starting maintenance tasks at {Time}", DateTime.UtcNow);

            // Archive old call logs (older than 90 days)
            await ArchiveOldCallLogsAsync();

            // Clean up expired DNC entries
            await CleanupExpiredDncEntriesAsync();

            // Update campaign statistics
            await UpdateCampaignStatisticsAsync();

            // Optimize database (update statistics, rebuild indexes, etc.)
            await OptimizeDatabaseAsync();

            _logger.LogInformation("Maintenance tasks completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in maintenance tasks timer function");
        }
    }

    /// <summary>
    /// HTTP trigger for manual campaign processing
    /// Allows administrators to manually trigger campaign processing
    /// </summary>
    [Function("ManualCampaignProcess")]
    public async Task<object> ManualCampaignProcess(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "campaigns/{campaignId}/process")] 
        HttpRequestData req,
        int campaignId,
        FunctionContext context)
    {
        try
        {
            _logger.LogInformation("Manual campaign processing requested for campaign {CampaignId}", campaignId);

            var result = await ProcessCampaignAsync(campaignId);

            return new
            {
                success = result.Success,
                message = result.Message,
                errorMessage = result.ErrorMessage,
                totalLeadsProcessed = result.TotalLeadsProcessed,
                successfulCalls = result.SuccessfulCalls,
                failedCalls = result.FailedCalls,
                processedAt = result.ProcessedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in manual campaign processing for campaign {CampaignId}", campaignId);
            return new
            {
                success = false,
                errorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// HTTP trigger for manual DNC scrubbing
    /// Allows administrators to manually trigger DNC scrubbing
    /// </summary>
    [Function("ManualDncScrub")]
    public async Task<object> ManualDncScrub(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "campaigns/{campaignId}/dnc-scrub")] 
        HttpRequestData req,
        int campaignId,
        FunctionContext context)
    {
        try
        {
            _logger.LogInformation("Manual DNC scrubbing requested for campaign {CampaignId}", campaignId);

            var result = await PerformDncScrubAsync(campaignId);

            return new
            {
                success = result.Success,
                errorMessage = result.ErrorMessage,
                totalLeadsProcessed = result.TotalLeadsProcessed,
                dncMatchesFound = result.DncMatchesFound,
                leadsUpdated = result.LeadsUpdated,
                processedAt = result.ProcessedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in manual DNC scrubbing for campaign {CampaignId}", campaignId);
            return new
            {
                success = false,
                errorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Gets list of active campaigns that should be processed
    /// </summary>
    private async Task<List<int>> GetActiveCampaignsForProcessingAsync()
    {
        // Query the database for active campaigns
        var campaigns = await _context.Campaigns
            .Where(c => c.IsActive)
            .Select(c => c.Id)
            .ToListAsync();
        
        return campaigns;
    }

    /// <summary>
    /// Archives old call logs to reduce database size
    /// </summary>
    private async Task ArchiveOldCallLogsAsync()
    {
        _logger.LogInformation("Archiving old call logs...");
        // Implementation would move old call logs to archive table or delete them
        await Task.Delay(100); // Placeholder
    }

    /// <summary>
    /// Removes expired DNC entries
    /// </summary>
    private async Task CleanupExpiredDncEntriesAsync()
    {
        _logger.LogInformation("Cleaning up expired DNC entries...");
        // Implementation would remove expired DNC numbers
        await Task.Delay(100); // Placeholder
    }

    /// <summary>
    /// Updates campaign statistics
    /// </summary>
    private async Task UpdateCampaignStatisticsAsync()
    {
        _logger.LogInformation("Updating campaign statistics...");
        // Implementation would recalculate campaign performance metrics
        await Task.Delay(100); // Placeholder
    }

    /// <summary>
    /// Optimizes database performance
    /// </summary>
    private async Task OptimizeDatabaseAsync()
    {
        _logger.LogInformation("Optimizing database...");
        // Implementation would update statistics, rebuild indexes, etc.
        await Task.Delay(100); // Placeholder
    }

    /// <summary>
    /// Processes a single campaign for outbound dialing
    /// </summary>
    private async Task<CampaignProcessingResult> ProcessCampaignAsync(int campaignId)
    {
        try
        {
            // Get campaign with its lists and settings
            var campaign = await _context.Campaigns
                .Include(c => c.CampaignLists)
                .ThenInclude(cl => cl.List)
                .FirstOrDefaultAsync(c => c.Id == campaignId && c.IsActive);

            if (campaign == null)
            {
                return new CampaignProcessingResult
                {
                    Success = false,
                    ErrorMessage = $"Campaign {campaignId} not found or inactive"
                };
            }

            // Check if campaign is within allowed calling hours
            if (!IsWithinAllowedCallingHours(campaign))
            {
                return new CampaignProcessingResult
                {
                    Success = true,
                    Message = "Campaign outside allowed calling hours"
                };
            }

            var totalLeadsProcessed = 0;
            var successfulCalls = 0;
            var failedCalls = 0;

            // Process each list in the campaign
            foreach (var campaignList in campaign.CampaignLists.Where(cl => cl.IsActive))
            {
                var leads = await GetLeadsToCallAsync(campaignList.ListId, campaignId);
                totalLeadsProcessed += leads.Count;

                foreach (var lead in leads)
                {
                    try
                    {
                        // Validate phone number before calling
                        if (!ValidationUtilities.IsValidPhoneNumber(lead.PrimaryPhone))
                        {
                            _logger.LogWarning("Invalid phone number for lead {LeadId}: {Phone}", 
                                lead.Id, lead.PrimaryPhone);
                            failedCalls++;
                            continue;
                        }

                        // Check DNC status
                        if (await IsOnDncListAsync(lead.PrimaryPhone))
                        {
                            await UpdateLeadExclusionStatusAsync(lead.Id, true, "DNC List");
                            continue;
                        }

                        // Initiate call through Communication Service
                        var callConnectionId = await _communicationService.InitiateOutboundCallAsync(
                            lead.PrimaryPhone, 
                            campaign.Id.ToString(),
                            lead.Id.ToString(),
                            new Uri("https://your-webhook-endpoint.com/call-events")); // TODO: Configure webhook endpoint

                        if (!string.IsNullOrEmpty(callConnectionId))
                        {
                            successfulCalls++;
                            await LogCallAttemptAsync(lead.Id, campaignId, callConnectionId, "Initiated");
                        }
                        else
                        {
                            failedCalls++;
                            await LogCallAttemptAsync(lead.Id, campaignId, null, "Failed", "Call initiation failed");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing lead {LeadId} in campaign {CampaignId}", 
                            lead.Id, campaignId);
                        failedCalls++;
                    }
                }
            }

            return new CampaignProcessingResult
            {
                Success = true,
                TotalLeadsProcessed = totalLeadsProcessed,
                SuccessfulCalls = successfulCalls,
                FailedCalls = failedCalls,
                ProcessedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing campaign {CampaignId}", campaignId);
            return new CampaignProcessingResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Performs DNC scrubbing for a campaign
    /// </summary>
    private async Task<DncScrubResult> PerformDncScrubAsync(int campaignId)
    {
        try
        {
            var campaign = await _context.Campaigns
                .Include(c => c.CampaignLists)
                .ThenInclude(cl => cl.List)
                .FirstOrDefaultAsync(c => c.Id == campaignId);

            if (campaign == null)
            {
                return new DncScrubResult
                {
                    Success = false,
                    ErrorMessage = $"Campaign {campaignId} not found"
                };
            }

            var totalProcessed = 0;
            var dncMatches = 0;
            var leadsUpdated = 0;

            foreach (var campaignList in campaign.CampaignLists)
            {
                var leads = await _context.Leads
                    .Where(l => l.ListId == campaignList.ListId && !l.IsExcluded)
                    .ToListAsync();

                totalProcessed += leads.Count;

                foreach (var lead in leads)
                {
                    if (await IsOnDncListAsync(lead.PrimaryPhone))
                    {
                        dncMatches++;
                        await UpdateLeadExclusionStatusAsync(lead.Id, true, "DNC List");
                        leadsUpdated++;
                    }
                }
            }

            return new DncScrubResult
            {
                Success = true,
                TotalLeadsProcessed = totalProcessed,
                DncMatchesFound = dncMatches,
                LeadsUpdated = leadsUpdated,
                ProcessedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing DNC scrub for campaign {CampaignId}", campaignId);
            return new DncScrubResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Gets leads that are ready to be called for a specific list
    /// </summary>
    private async Task<List<Lead>> GetLeadsToCallAsync(int listId, int campaignId)
    {
        // Get leads that haven't been called recently and aren't excluded
        return await _context.Leads
            .Where(l => l.ListId == listId && 
                       !l.IsExcluded && 
                       l.Status != "Completed")
            .OrderBy(l => l.ModifyDate) // Call oldest first
            .Take(100) // Limit batch size
            .ToListAsync();
    }

    /// <summary>
    /// Checks if a phone number is on the DNC list
    /// </summary>
    private async Task<bool> IsOnDncListAsync(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // Clean phone number for comparison
        var cleanPhone = System.Text.RegularExpressions.Regex.Replace(phoneNumber, @"[^\d]", "");

        return await _context.DncNumbers
            .AnyAsync(d => d.PhoneNumber == cleanPhone && 
                          (d.ExpiresAt == null || d.ExpiresAt > DateTime.UtcNow));
    }

    /// <summary>
    /// Updates a lead's exclusion status
    /// </summary>
    private async Task UpdateLeadExclusionStatusAsync(int leadId, bool isExcluded, string reason)
    {
        var lead = await _context.Leads.FindAsync(leadId);
        if (lead != null)
        {
            lead.IsExcluded = isExcluded;
            lead.ExclusionReason = reason;
            lead.ExcludedAt = isExcluded ? DateTime.UtcNow : null;
            lead.ModifyDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Logs a call attempt
    /// </summary>
    private async Task LogCallAttemptAsync(int leadId, int campaignId, string? callId, string status, string? errorMessage = null)
    {
        var callLog = new CallLog
        {
            LeadId = leadId,
            CampaignId = campaignId,
            CallId = callId,
            PhoneNumber = "", // Will be populated from lead
            StartedAt = DateTime.UtcNow,
            CallStatus = status,
            Notes = errorMessage
        };

        _context.CallLogs.Add(callLog);

        // Update lead modification date
        var lead = await _context.Leads.FindAsync(leadId);
        if (lead != null)
        {
            lead.ModifyDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Checks if campaign is within allowed calling hours
    /// </summary>
    private bool IsWithinAllowedCallingHours(Campaign campaign)
    {
        var now = DateTime.Now;
        
        // Parse local start/end times from campaign
        if (TimeSpan.TryParse(campaign.CallStartTime, out var startTime) &&
            TimeSpan.TryParse(campaign.CallEndTime, out var endTime))
        {
            var currentTime = now.TimeOfDay;
            
            // Handle time ranges that cross midnight
            if (startTime <= endTime)
            {
                return currentTime >= startTime && currentTime <= endTime;
            }
            else
            {
                return currentTime >= startTime || currentTime <= endTime;
            }
        }

        // Default to allowing calls during business hours if not configured
        return now.Hour >= 9 && now.Hour <= 17;
    }
}

/// <summary>
/// Result object for campaign processing operations
/// </summary>
public class CampaignProcessingResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }
    public int TotalLeadsProcessed { get; set; }
    public int SuccessfulCalls { get; set; }
    public int FailedCalls { get; set; }
    public DateTime ProcessedAt { get; set; }
}

/// <summary>
/// Result object for DNC scrubbing operations
/// </summary>
public class DncScrubResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int TotalLeadsProcessed { get; set; }
    public int DncMatchesFound { get; set; }
    public int LeadsUpdated { get; set; }
    public DateTime ProcessedAt { get; set; }
}
