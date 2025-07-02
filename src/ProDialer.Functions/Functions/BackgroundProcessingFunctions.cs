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
    private readonly LeadFilteringService _leadFilteringService;

    public BackgroundProcessingFunctions(
        ILogger<BackgroundProcessingFunctions> logger,
        ProDialerDbContext context,
        TableStorageService tableStorageService,
        CommunicationService communicationService,
        LeadFilteringService leadFilteringService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _tableStorageService = tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
        _communicationService = communicationService ?? throw new ArgumentNullException(nameof(communicationService));
        _leadFilteringService = leadFilteringService ?? throw new ArgumentNullException(nameof(leadFilteringService));
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

            // Get leads that need timezone validation
            var leadsNeedingTimezone = await _context.Leads
                .Where(l => string.IsNullOrEmpty(l.TimeZone) && !string.IsNullOrEmpty(l.PrimaryPhone))
                .Take(5000) // Process in batches
                .ToListAsync();

            _logger.LogInformation("Found {LeadCount} leads requiring timezone validation", leadsNeedingTimezone.Count);

            var updatedCount = 0;

            foreach (var lead in leadsNeedingTimezone)
            {
                try
                {
                    // Detect timezone from area code
                    var timezone = ValidationUtilities.GetTimezoneFromAreaCode(lead.PrimaryPhone);
                    if (timezone != null)
                    {
                        lead.TimeZone = timezone.Id;
                        lead.ModifyDate = DateTime.UtcNow;
                        updatedCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error detecting timezone for lead {LeadId}: {Phone}", 
                        lead.Id, lead.PrimaryPhone);
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Timezone validation completed: {UpdatedCount} leads updated", updatedCount);
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

            // Get leads with unvalidated phone numbers
            var leadsToValidate = await _context.Leads
                .Where(l => l.PhoneValidationStatus == "UNVALIDATED" || l.PhoneValidationStatus == null)
                .Where(l => !string.IsNullOrEmpty(l.PrimaryPhone))
                .Take(1000) // Process in batches to avoid timeouts
                .ToListAsync();

            _logger.LogInformation("Found {LeadCount} leads requiring phone validation", leadsToValidate.Count);

            var validatedCount = 0;
            var invalidCount = 0;

            foreach (var lead in leadsToValidate)
            {
                try
                {
                    // Validate phone number using ValidationUtilities
                    var isValid = ValidationUtilities.IsValidPhoneNumber(lead.PrimaryPhone);
                    var isCallable = ValidationUtilities.IsCallableNumber(lead.PrimaryPhone);
                    var isLikelyMobile = ValidationUtilities.IsLikelyMobileNumber(lead.PrimaryPhone);
                    var isLikelyDnc = ValidationUtilities.IsLikelyDncNumber(lead.PrimaryPhone);
                    
                    // Update lead with validation results
                    lead.PhoneValidationStatus = isValid && isCallable ? "VALID" : "INVALID";
                    lead.PhoneNumberRaw = ValidationUtilities.NormalizePhoneNumber(lead.PrimaryPhone);
                    
                    // Set timezone based on area code
                    var timezone = ValidationUtilities.GetTimezoneFromAreaCode(lead.PrimaryPhone);
                    if (timezone != null)
                    {
                        lead.TimeZone = timezone.Id;
                    }

                    // Update mobile indicator
                    if (isLikelyMobile)
                    {
                        lead.PhoneType = "MOBILE";
                    }

                    // Auto-exclude if likely DNC or invalid
                    if (isLikelyDnc || !isCallable)
                    {
                        lead.IsExcluded = true;
                        lead.ExclusionReason = isLikelyDnc ? "Likely DNC" : "Invalid/Uncallable Number";
                        lead.ExcludedAt = DateTime.UtcNow;
                    }

                    lead.ModifyDate = DateTime.UtcNow;

                    if (isValid && isCallable)
                        validatedCount++;
                    else
                        invalidCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error validating phone number for lead {LeadId}: {Phone}", 
                        lead.Id, lead.PrimaryPhone);
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Phone validation completed: {ValidCount} valid, {InvalidCount} invalid", 
                validatedCount, invalidCount);
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
    /// Recycles old leads that meet recycling criteria
    /// Runs every 4 hours to refresh the lead pool
    /// </summary>
    [Function("LeadRecycling")]
    public async Task LeadRecycling(
        [TimerTrigger("0 0 */4 * * *")] object timer, // Every 4 hours
        FunctionContext context)
    {
        try
        {
            _logger.LogInformation("Starting lead recycling at {Time}", DateTime.UtcNow);

            var activeCampaigns = await GetActiveCampaignsForProcessingAsync();
            var totalRecycled = 0;

            foreach (var campaignId in activeCampaigns)
            {
                try
                {
                    var recycledCount = await _leadFilteringService.RecycleLeadsAsync(campaignId);
                    totalRecycled += recycledCount;

                    _logger.LogInformation("Recycled {RecycledCount} leads for campaign {CampaignId}", 
                        recycledCount, campaignId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error recycling leads for campaign {CampaignId}", campaignId);
                }
            }

            _logger.LogInformation("Lead recycling completed: {TotalRecycled} leads recycled across {CampaignCount} campaigns", 
                totalRecycled, activeCampaigns.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error in lead recycling timer function");
        }
    }

    /// <summary>
    /// Updates lead quality scores based on call performance
    /// Runs daily to maintain accurate lead scoring
    /// </summary>
    [Function("UpdateLeadQualityScores")]
    public async Task UpdateLeadQualityScores(
        [TimerTrigger("0 0 1 * * *")] object timer, // Daily at 1 AM
        FunctionContext context)
    {
        try
        {
            _logger.LogInformation("Starting lead quality score updates at {Time}", DateTime.UtcNow);

            var activeCampaigns = await GetActiveCampaignsForProcessingAsync();

            foreach (var campaignId in activeCampaigns)
            {
                try
                {
                    await _leadFilteringService.UpdateLeadQualityScoresAsync(campaignId);
                    _logger.LogInformation("Updated lead quality scores for campaign {CampaignId}", campaignId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating quality scores for campaign {CampaignId}", campaignId);
                }
            }

            _logger.LogInformation("Lead quality score updates completed for {CampaignCount} campaigns", 
                activeCampaigns.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error in lead quality score update timer function");
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
        
        try
        {
            var archiveDate = DateTime.UtcNow.AddDays(-90); // Archive logs older than 90 days
            
            var oldCallLogs = await _context.CallLogs
                .Where(cl => cl.StartedAt < archiveDate)
                .CountAsync();

            if (oldCallLogs > 0)
            {
                // In a production system, you might move these to an archive table first
                // For now, we'll just delete them to free up space
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM CallLogs WHERE StartedAt < {0}", archiveDate);

                _logger.LogInformation("Archived {CallLogCount} old call logs", oldCallLogs);
            }
            else
            {
                _logger.LogInformation("No old call logs found to archive");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving old call logs");
        }
    }

    /// <summary>
    /// Removes expired DNC entries
    /// </summary>
    private async Task CleanupExpiredDncEntriesAsync()
    {
        _logger.LogInformation("Cleaning up expired DNC entries...");
        
        try
        {
            var expiredCount = await _context.DncNumbers
                .Where(d => d.ExpiresAt != null && d.ExpiresAt < DateTime.UtcNow)
                .CountAsync();

            if (expiredCount > 0)
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM DncNumbers WHERE ExpiresAt IS NOT NULL AND ExpiresAt < {0}", DateTime.UtcNow);

                _logger.LogInformation("Cleaned up {ExpiredCount} expired DNC entries", expiredCount);
            }
            else
            {
                _logger.LogInformation("No expired DNC entries found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired DNC entries");
        }
    }

    /// <summary>
    /// Updates campaign statistics
    /// </summary>
    private async Task UpdateCampaignStatisticsAsync()
    {
        _logger.LogInformation("Updating campaign statistics...");
        
        try
        {
            var campaigns = await _context.Campaigns.Where(c => c.IsActive).ToListAsync();
            
            foreach (var campaign in campaigns)
            {
                // Calculate daily statistics
                var today = DateTime.UtcNow.Date;
                var tomorrow = today.AddDays(1);

                var dailyStats = await _context.CallLogs
                    .Where(cl => cl.CampaignId == campaign.Id && 
                                cl.StartedAt >= today && 
                                cl.StartedAt < tomorrow)
                    .GroupBy(cl => 1)
                    .Select(g => new {
                        TotalCalls = g.Count(),
                        Contacts = g.Count(cl => cl.CallStatus == "Connected" || cl.CallStatus == "Answered"),
                        Drops = g.Count(cl => cl.CallStatus == "Drop" || cl.CallStatus == "Abandoned")
                    })
                    .FirstOrDefaultAsync();

                if (dailyStats != null)
                {
                    campaign.TotalDialedToday = dailyStats.TotalCalls;
                    campaign.TotalContactsToday = dailyStats.Contacts;
                    
                    // Calculate drop rate
                    if (dailyStats.TotalCalls > 0)
                    {
                        campaign.CurrentDropRate = (decimal)dailyStats.Drops / dailyStats.TotalCalls * 100;
                    }
                }

                campaign.StatsLastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated statistics for {CampaignCount} campaigns", campaigns.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating campaign statistics");
        }
    }

    /// <summary>
    /// Optimizes database performance
    /// </summary>
    private async Task OptimizeDatabaseAsync()
    {
        _logger.LogInformation("Optimizing database...");
        
        try
        {
            // Update database statistics for better query performance
            await _context.Database.ExecuteSqlRawAsync("UPDATE STATISTICS Leads");
            await _context.Database.ExecuteSqlRawAsync("UPDATE STATISTICS CallLogs");
            await _context.Database.ExecuteSqlRawAsync("UPDATE STATISTICS Campaigns");
            
            _logger.LogInformation("Database optimization completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing database");
        }
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
                        var callRequest = new OutboundCallRequest
                        {
                            ToPhoneNumber = lead.PrimaryPhone,
                            FromPhoneNumber = "+18005551234", // TODO: Configure from campaign or settings
                            CampaignId = campaign.Id,
                            LeadId = lead.Id,
                            CallLogId = 0 // Will be set after call initiation
                        };

                        var callResult = await _communicationService.InitiateOutboundCallAsync(callRequest);

                        if (callResult.Success && !string.IsNullOrEmpty(callResult.CallId))
                        {
                            successfulCalls++;
                            await LogCallAttemptAsync(lead.Id, campaignId, callResult.CallId, "Initiated");
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
        // Use the advanced lead filtering service
        var filteredQuery = await _leadFilteringService.ApplyLeadFiltersAsync(listId, campaignId);
        
        // Apply batch size limit
        var batchSize = 50; // Default batch size
        
        return await filteredQuery.Take(batchSize).ToListAsync();
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
        // Get the lead to update call tracking
        var lead = await _context.Leads.FindAsync(leadId);
        if (lead == null) return;

        var callLog = new CallLog
        {
            LeadId = leadId,
            CampaignId = campaignId,
            CallId = callId,
            PhoneNumber = lead.PrimaryPhone,
            StartedAt = DateTime.UtcNow,
            CallStatus = status,
            Notes = errorMessage
        };

        _context.CallLogs.Add(callLog);

        // Update lead call tracking
        lead.CallAttempts++;
        lead.LastCalledAt = DateTime.UtcNow;
        lead.ModifyDate = DateTime.UtcNow;
        lead.CalledSinceLastReset = true;

        // Update status based on call outcome
        if (status == "Initiated")
        {
            lead.Status = "CALLED";
        }
        else if (status == "Failed")
        {
            lead.Status = "B"; // Busy/Failed status in VICIdial convention
        }

        // Set next call time based on campaign retry settings
        if (status == "Failed" || status == "Busy")
        {
            var campaign = await _context.Campaigns.FindAsync(campaignId);
            if (campaign != null)
            {
                // Use campaign's retry delay or default to 4 hours
                var retryHours = 4; // Default
                lead.NextCallAt = DateTime.UtcNow.AddHours(retryHours);
            }
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

    /// <summary>
    /// Gets timezone information from a postal code (US/Canada)
    /// </summary>
    /// <param name="postalCode">Postal code to analyze</param>
    /// <returns>TimeZoneInfo or null if not determinable</returns>
    private TimeZoneInfo? GetTimezoneFromPostalCode(string postalCode)
    {
        if (string.IsNullOrWhiteSpace(postalCode))
            return null;

        // Simple postal code to timezone mapping for major regions
        // This is a basic implementation - production would use a comprehensive postal code database
        var code = postalCode.Trim().ToUpper();

        // US ZIP code patterns
        if (code.Length == 5 && int.TryParse(code, out var zip))
        {
            return zip switch
            {
                // Eastern Time Zone
                >= 00501 and <= 02199 => TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"), // Northeast
                >= 03000 and <= 04999 => TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"), // New England
                >= 07000 and <= 08999 => TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"), // NJ
                >= 10000 and <= 14999 => TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"), // NY
                >= 15000 and <= 19999 => TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"), // PA
                >= 20000 and <= 24999 => TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"), // DC/MD/VA
                >= 27000 and <= 28999 => TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"), // NC/SC
                >= 29000 and <= 29999 => TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"), // SC
                >= 30000 and <= 31999 => TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"), // GA
                >= 32000 and <= 34999 => TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"), // FL

                // Central Time Zone
                >= 35000 and <= 36999 => TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"), // AL
                >= 37000 and <= 38999 => TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"), // AR
                >= 50000 and <= 52999 => TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"), // IA
                >= 60000 and <= 62999 => TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"), // IL
                >= 63000 and <= 65999 => TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"), // MO
                >= 70000 and <= 71999 => TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"), // LA
                >= 73000 and <= 74999 => TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"), // TX
                >= 75000 and <= 79999 => TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"), // TX

                // Mountain Time Zone
                >= 80000 and <= 81999 => TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"), // CO
                >= 82000 and <= 83999 => TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"), // WY
                >= 84000 and <= 84999 => TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"), // UT
                >= 85000 and <= 86999 => TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"), // AZ
                >= 87000 and <= 88999 => TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"), // NM
                >= 59000 and <= 59999 => TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"), // MT

                // Pacific Time Zone
                >= 90000 and <= 96999 => TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"), // CA
                >= 97000 and <= 97999 => TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"), // OR
                >= 98000 and <= 99499 => TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"), // WA

                // Alaska
                >= 99500 and <= 99999 => TimeZoneInfo.FindSystemTimeZoneById("Alaskan Standard Time"),

                _ => null
            };
        }

        // Canadian postal codes (first letter indicates province/territory)
        if (code.Length >= 3)
        {
            var province = code.Substring(0, 1);
            return province switch
            {
                "A" => TimeZoneInfo.FindSystemTimeZoneById("Atlantic Standard Time"), // Newfoundland/Maritime
                "B" or "C" or "V" => TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"), // BC
                "T" => TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"), // Alberta
                "S" => TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"), // Saskatchewan
                "R" => TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"), // Manitoba
                "P" => TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"), // Ontario
                "G" or "H" or "J" => TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"), // Quebec
                "E" => TimeZoneInfo.FindSystemTimeZoneById("Atlantic Standard Time"), // New Brunswick
                "K" or "L" or "M" or "N" => TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"), // Ontario
                _ => null
            };
        }

        return null;
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
