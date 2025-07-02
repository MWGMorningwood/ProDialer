using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProDialer.Functions.Data;
using ProDialer.Shared.Models;
using System.Text.Json;

namespace ProDialer.Functions.Services;

/// <summary>
/// Service for advanced lead filtering and selection algorithms
/// Implements VICIdial-style lead filtering capabilities
/// </summary>
public class LeadFilteringService
{
    private readonly ILogger<LeadFilteringService> _logger;
    private readonly ProDialerDbContext _context;

    public LeadFilteringService(ILogger<LeadFilteringService> logger, ProDialerDbContext context)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Applies lead filtering based on campaign and list settings
    /// </summary>
    /// <param name="listId">List ID to filter</param>
    /// <param name="campaignId">Campaign ID for context</param>
    /// <returns>Filtered lead query</returns>
    public async Task<IQueryable<Lead>> ApplyLeadFiltersAsync(int listId, int campaignId)
    {
        var campaign = await _context.Campaigns
            .Include(c => c.CampaignLists.Where(cl => cl.ListId == listId))
            .FirstOrDefaultAsync(c => c.Id == campaignId);

        if (campaign == null)
            throw new InvalidOperationException($"Campaign {campaignId} not found");

        var campaignList = campaign.CampaignLists.FirstOrDefault();
        var list = await _context.Lists.FindAsync(listId);

        var query = _context.Leads.Where(l => l.ListId == listId);

        // Apply basic exclusion filters
        query = ApplyBasicFilters(query);

        // Apply timezone-based calling restrictions
        query = ApplyTimezoneFilters(query, campaign);

        // Apply lead lifecycle filters
        query = ApplyLifecycleFilters(query, campaign);

        // Apply phone validation filters
        query = ApplyPhoneValidationFilters(query);

        // Apply DNC filters
        query = await ApplyDncFiltersAsync(query);

        // Apply custom field filters if defined
        // Note: Custom filtering would be implemented based on actual List model properties
        
        // Apply standard ordering (priority-based)
        query = query.OrderByDescending(l => l.Priority)
                    .ThenBy(l => l.LastCalledAt ?? DateTime.MinValue)
                    .ThenBy(l => l.ModifyDate);

        return query;
    }

    /// <summary>
    /// Applies basic lead exclusion filters
    /// </summary>
    private IQueryable<Lead> ApplyBasicFilters(IQueryable<Lead> query)
    {
        return query.Where(l => 
            !l.IsExcluded &&
            l.Status != "Completed" &&
            l.Status != "Do Not Call" &&
            l.Status != "DNC" &&
            !string.IsNullOrEmpty(l.PrimaryPhone));
    }

    /// <summary>
    /// Applies timezone-based calling restrictions
    /// </summary>
    private IQueryable<Lead> ApplyTimezoneFilters(IQueryable<Lead> query, Campaign campaign)
    {
        var now = DateTime.UtcNow;

        // Parse campaign calling hours
        if (TimeSpan.TryParse(campaign.CallStartTime, out var startTime) &&
            TimeSpan.TryParse(campaign.CallEndTime, out var endTime))
        {
            // For now, apply a simple time filter
            // In production, you'd need to check each lead's timezone individually
            query = query.Where(l => l.TimeZone == null || 
                               l.NextCallAt == null || 
                               l.NextCallAt <= now);
        }

        return query;
    }

    /// <summary>
    /// Applies lead lifecycle and recycling filters
    /// </summary>
    private IQueryable<Lead> ApplyLifecycleFilters(IQueryable<Lead> query, Campaign campaign)
    {
        var now = DateTime.UtcNow;

        // Apply call attempt limits
        query = query.Where(l => l.CallAttempts < campaign.MaxCallAttempts);

        // Apply retry delay filters
        var retryHours = 4; // Default retry interval
        var earliestRetryTime = now.AddHours(-retryHours);
        query = query.Where(l => l.LastCalledAt == null || l.LastCalledAt <= earliestRetryTime);

        // Apply recycling rules if enabled
        if (campaign.EnableAutoRecycling)
        {
            var maxRecycles = campaign.MaxRecycleCount;
            query = query.Where(l => l.RecycleCount < maxRecycles);
        }

        return query;
    }

    /// <summary>
    /// Applies phone number validation filters
    /// </summary>
    private IQueryable<Lead> ApplyPhoneValidationFilters(IQueryable<Lead> query)
    {
        return query.Where(l => 
            l.PhoneValidationStatus == "VALID" || 
            l.PhoneValidationStatus == null || // Allow unvalidated for now
            l.PhoneValidationStatus == "UNVALIDATED");
    }

    /// <summary>
    /// Applies Do Not Call filters
    /// </summary>
    private async Task<IQueryable<Lead>> ApplyDncFiltersAsync(IQueryable<Lead> query)
    {
        // Get list of DNC phone numbers for quick lookup
        var dncNumbers = await _context.DncNumbers
            .Where(d => d.ExpiresAt == null || d.ExpiresAt > DateTime.UtcNow)
            .Select(d => d.PhoneNumber)
            .ToListAsync();

        if (dncNumbers.Any())
        {
            query = query.Where(l => !dncNumbers.Contains(l.PhoneNumberRaw ?? l.PrimaryPhone));
        }

        return query;
    }

    /// <summary>
    /// Applies custom filtering rules from JSON configuration
    /// </summary>
    private IQueryable<Lead> ApplyCustomFilters(IQueryable<Lead> query, string filterRulesJson)
    {
        try
        {
            var filterRules = JsonSerializer.Deserialize<LeadFilterRules>(filterRulesJson);
            if (filterRules == null) return query;

            // Apply status filters
            if (filterRules.ExcludeStatuses?.Any() == true)
            {
                query = query.Where(l => !filterRules.ExcludeStatuses.Contains(l.Status ?? ""));
            }

            if (filterRules.IncludeStatuses?.Any() == true)
            {
                query = query.Where(l => filterRules.IncludeStatuses.Contains(l.Status ?? ""));
            }

            // Apply priority filters
            if (filterRules.MinPriority.HasValue)
            {
                query = query.Where(l => l.Priority >= filterRules.MinPriority.Value);
            }

            if (filterRules.MaxPriority.HasValue)
            {
                query = query.Where(l => l.Priority <= filterRules.MaxPriority.Value);
            }

            // Apply quality score filters
            if (filterRules.MinQualityScore.HasValue)
            {
                query = query.Where(l => l.QualityScore >= filterRules.MinQualityScore.Value);
            }

            // Apply date range filters
            if (filterRules.CreatedAfter.HasValue)
            {
                query = query.Where(l => l.CreatedAt >= filterRules.CreatedAfter.Value);
            }

            if (filterRules.CreatedBefore.HasValue)
            {
                query = query.Where(l => l.CreatedAt <= filterRules.CreatedBefore.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error applying custom filter rules: {FilterRules}", filterRulesJson);
        }

        return query;
    }

    /// <summary>
    /// Applies ordering rules based on campaign configuration
    /// </summary>
    private IQueryable<Lead> ApplyOrderingRules(IQueryable<Lead> query, string orderBy)
    {
        return orderBy.ToLower() switch
        {
            "priority" => query.OrderByDescending(l => l.Priority)
                              .ThenBy(l => l.LastCalledAt ?? DateTime.MinValue),
            
            "last_call_time" => query.OrderBy(l => l.LastCalledAt ?? DateTime.MinValue)
                                    .ThenByDescending(l => l.Priority),
            
            "quality_score" => query.OrderByDescending(l => l.QualityScore)
                                   .ThenByDescending(l => l.Priority),
            
            "created_date" => query.OrderBy(l => l.CreatedAt)
                                  .ThenByDescending(l => l.Priority),
            
            "random" => query.OrderBy(l => Guid.NewGuid()), // Not ideal for large datasets
            
            _ => query.OrderByDescending(l => l.Priority)
                     .ThenBy(l => l.LastCalledAt ?? DateTime.MinValue)
        };
    }

    /// <summary>
    /// Processes lead recycling for completed campaigns
    /// </summary>
    public async Task<int> ProcessLeadRecyclingAsync(int campaignId)
    {
        _logger.LogInformation("Processing lead recycling for campaign {CampaignId}", campaignId);

        var campaign = await _context.Campaigns.FindAsync(campaignId);
        if (campaign == null || !campaign.EnableAutoRecycling)
            return 0;

        var maxRecycles = campaign.MaxRecycleCount;
        var recycleStatuses = new[] { "B", "NA", "BUSY", "NO ANSWER" }; // VICIdial status codes

        var leadsToRecycle = await _context.Leads
            .Where(l => recycleStatuses.Contains(l.Status ?? "") && 
                       l.RecycleCount < maxRecycles &&
                       l.CallAttempts >= campaign.MaxCallAttempts)
            .ToListAsync();

        var recycledCount = 0;

        foreach (var lead in leadsToRecycle)
        {
            lead.RecycleCount++;
            lead.CallAttempts = 0; // Reset call attempts
            lead.Status = "NEW"; // Reset status
            lead.LastCalledAt = null; // Clear last called time
            lead.NextCallAt = DateTime.UtcNow.AddHours(GetRecyclingDelayHours(campaign));
            lead.ModifyDate = DateTime.UtcNow;
            recycledCount++;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Recycled {RecycledCount} leads for campaign {CampaignId}", 
            recycledCount, campaignId);

        return recycledCount;
    }

    /// <summary>
    /// Gets the recycling delay hours from campaign settings or default
    /// </summary>
    private int GetRecyclingDelayHours(Campaign campaign)
    {
        // Default delay
        var defaultDelay = 24;

        // Try to parse RecyclingRules JSON to get delay
        if (!string.IsNullOrEmpty(campaign.RecyclingRules))
        {
            try
            {
                // Simple parsing - in production you'd use System.Text.Json
                // Look for "delay_hours": value pattern
                var rules = campaign.RecyclingRules;
                var delayIndex = rules.IndexOf("delay_hours");
                if (delayIndex > 0)
                {
                    var startIndex = rules.IndexOf(':', delayIndex) + 1;
                    var endIndex = rules.IndexOf(',', startIndex);
                    if (endIndex == -1) endIndex = rules.IndexOf('}', startIndex);
                    
                    if (startIndex > 0 && endIndex > startIndex)
                    {
                        var delayStr = rules.Substring(startIndex, endIndex - startIndex).Trim();
                        if (int.TryParse(delayStr, out var delay))
                        {
                            return delay;
                        }
                    }
                }
            }
            catch
            {
                // If parsing fails, use default
            }
        }

        return defaultDelay;
    }

    /// <summary>
    /// Updates lead quality scores based on validation results and call history
    /// </summary>
    /// <param name="campaignId">Campaign to update quality scores for</param>
    /// <returns>Number of leads updated</returns>
    public async Task<int> UpdateLeadQualityScoresAsync(int campaignId)
    {
        try
        {
            var campaign = await _context.Campaigns.FindAsync(campaignId);
            if (campaign == null)
            {
                _logger.LogWarning("Campaign {CampaignId} not found for quality score update", campaignId);
                return 0;
            }

            var listIds = await _context.CampaignLists
                .Where(cl => cl.CampaignId == campaignId)
                .Select(cl => cl.ListId)
                .ToListAsync();

            if (!listIds.Any())
            {
                _logger.LogInformation("No lists found for campaign {CampaignId}", campaignId);
                return 0;
            }

            var leads = await _context.Leads
                .Where(l => listIds.Contains(l.ListId))
                .ToListAsync();

            int updatedCount = 0;

            foreach (var lead in leads)
            {
                var oldScore = lead.QualityScore;
                var newScore = await CalculateLeadQualityScore(lead);

                if (newScore != oldScore)
                {
                    lead.QualityScore = newScore;
                    lead.ModifyDate = DateTime.UtcNow;
                    updatedCount++;
                }
            }

            if (updatedCount > 0)
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated quality scores for {UpdatedCount} leads in campaign {CampaignId}", 
                    updatedCount, campaignId);
            }

            return updatedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lead quality scores for campaign {CampaignId}", campaignId);
            throw;
        }
    }

    /// <summary>
    /// Calculates quality score for a lead based on various factors
    /// </summary>
    /// <param name="lead">Lead to calculate score for</param>
    /// <returns>Quality score (0-100)</returns>
    private async Task<int> CalculateLeadQualityScore(Lead lead)
    {
        int score = 50; // Base score

        // Phone validation score (20 points max)
        if (lead.PhoneValidationStatus == "VALID")
        {
            score += 20;
        }
        else if (lead.PhoneValidationStatus == "INVALID")
        {
            score -= 10;
        }

        // Mobile number bonus (5 points)
        if (lead.PhoneType == "MOBILE")
        {
            score += 5;
        }

        // Call history analysis (25 points max)
        var callLogs = await _context.CallLogs
            .Where(cl => cl.LeadId == lead.Id)
            .OrderByDescending(cl => cl.StartedAt)
            .Take(10)
            .ToListAsync();

        if (callLogs.Any())
        {
            var answeredCalls = callLogs.Count(cl => cl.CallStatus == "ANSWERED");
            var totalCalls = callLogs.Count;
            var answerRate = (double)answeredCalls / totalCalls;

            // Higher answer rate = higher score
            score += (int)(answerRate * 25);

            // Recent activity bonus
            var lastCall = callLogs.First().StartedAt;
            var daysSinceLastCall = (DateTime.UtcNow - lastCall).TotalDays;
            
            if (daysSinceLastCall <= 7)
                score += 5; // Recent contact bonus
            else if (daysSinceLastCall <= 30)
                score += 2;
        }

        // Exclusion penalties
        if (lead.IsExcluded == true)
            score -= 30;
        
        if (lead.IsExcluded == true)
            score -= 50;

        // Timezone validation bonus (5 points)
        if (!string.IsNullOrEmpty(lead.TimeZone))
            score += 5;

        // Ensure score is within valid range
        return Math.Max(0, Math.Min(100, score));
    }

    /// <summary>
    /// Alias method for compatibility with BackgroundProcessingFunctions
    /// </summary>
    /// <param name="campaignId">Campaign ID to recycle leads for</param>
    /// <returns>Number of leads recycled</returns>
    public async Task<int> RecycleLeadsAsync(int campaignId)
    {
        return await ProcessLeadRecyclingAsync(campaignId);
    }
}

/// <summary>
/// Configuration class for lead filtering rules
/// </summary>
public class LeadFilterRules
{
    public string[]? ExcludeStatuses { get; set; }
    public string[]? IncludeStatuses { get; set; }
    public int? MinPriority { get; set; }
    public int? MaxPriority { get; set; }
    public int? MinQualityScore { get; set; }
    public int? MaxQualityScore { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public string? CustomSqlFilter { get; set; }
}
