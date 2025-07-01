using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProDialer.Functions.Data;
using ProDialer.Shared.DTOs;
using ProDialer.Shared.Models;
using System.Net;
using System.Text.Json;

namespace ProDialer.Functions.Functions;

/// <summary>
/// Azure Functions for managing campaigns
/// Provides CRUD operations and campaign-specific functionality with comprehensive VICIdial feature support
/// </summary>
public class CampaignFunctions
{
    private readonly ProDialerDbContext _dbContext;
    private readonly ILogger<CampaignFunctions> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public CampaignFunctions(ProDialerDbContext dbContext, ILogger<CampaignFunctions> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    [Function("GetCampaigns")]
    public async Task<HttpResponseData> GetCampaigns([HttpTrigger(AuthorizationLevel.Function, "get", Route = "campaigns")] HttpRequestData req)
    {
        try
        {
            _logger.LogInformation("Retrieving campaigns");

            var campaigns = await _dbContext.Campaigns
                .OrderBy(c => c.Name)
                .Select(c => new CampaignSummaryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    DialMethod = c.DialMethod,
                    IsActive = c.IsActive,
                    DialingRatio = c.DialingRatio,
                    Priority = c.Priority,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    TotalDialedToday = c.TotalDialedToday,
                    TotalContactsToday = c.TotalContactsToday,
                    CurrentDropRate = c.CurrentDropRate,
                    StatsLastUpdated = c.StatsLastUpdated,
                    TotalLeads = 0, // Will be calculated separately for performance
                    CalledLeads = 0,
                    ContactedLeads = 0,
                    TotalLists = 0, // Will be calculated separately for performance
                    ActiveLists = 0
                })
                .ToListAsync();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(campaigns);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving campaigns");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("An error occurred while retrieving campaigns");
            return response;
        }
    }

    [Function("GetCampaignById")]
    public async Task<HttpResponseData> GetCampaignById([HttpTrigger(AuthorizationLevel.Function, "get", Route = "campaigns/{id:int}")] HttpRequestData req, int id)
    {
        try
        {
            _logger.LogInformation("Retrieving campaign with ID: {CampaignId}", id);

            var campaign = await _dbContext.Campaigns
                .FirstOrDefaultAsync(c => c.Id == id);

            if (campaign == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"Campaign with ID {id} not found");
                return notFoundResponse;
            }

            var campaignDto = new CampaignDto
            {
                Id = campaign.Id,
                Name = campaign.Name,
                Description = campaign.Description,
                DialMethod = campaign.DialMethod,
                DialingRatio = campaign.DialingRatio,
                ApplyRatioToIdleAgentsOnly = campaign.ApplyRatioToIdleAgentsOnly,
                AdaptiveMaxLevel = campaign.AdaptiveMaxLevel,
                DialTimeout = campaign.DialTimeout,
                MaxConcurrentCalls = campaign.MaxConcurrentCalls,
                DialPrefix = campaign.DialPrefix,
                ManualDialPrefix = campaign.ManualDialPrefix,
                ThreeWayDialPrefix = campaign.ThreeWayDialPrefix,
                ParkExtension = campaign.ParkExtension,
                ParkFileName = campaign.ParkFileName,
                VoicemailExtension = campaign.VoicemailExtension,
                AnsweringMachineExtension = campaign.AnsweringMachineExtension,
                AvailableOnlyRatioTally = campaign.AvailableOnlyRatioTally,
                AvailableOnlyTallyThreshold = campaign.AvailableOnlyTallyThreshold,
                NextAgentCall = campaign.NextAgentCall,
                AlternateNumberDialing = campaign.AlternateNumberDialing,
                ScheduledCallbacks = campaign.ScheduledCallbacks,
                AllowInbound = campaign.AllowInbound,
                ForceResetHopper = campaign.ForceResetHopper,
                HopperDuplicateCheck = campaign.HopperDuplicateCheck,
                GetCallLaunch = campaign.GetCallLaunch,
                TimeZone = campaign.TimeZone,
                CallStartTime = campaign.CallStartTime,
                CallEndTime = campaign.CallEndTime,
                AllowedDaysOfWeek = campaign.AllowedDaysOfWeek,
                RespectLeadTimeZone = campaign.RespectLeadTimeZone,
                MinCallInterval = campaign.MinCallInterval,
                MaxCallAttempts = campaign.MaxCallAttempts,
                CallAttemptDelay = campaign.CallAttemptDelay,
                EnableAnsweringMachineDetection = campaign.EnableAnsweringMachineDetection,
                AnsweringMachineAction = campaign.AnsweringMachineAction,
                AnsweringMachineMessage = campaign.AnsweringMachineMessage,
                LeadOrder = campaign.LeadOrder,
                RandomizeLeadOrder = campaign.RandomizeLeadOrder,
                LeadOrderSecondary = campaign.LeadOrderSecondary,
                HopperLevel = campaign.HopperLevel,
                OutboundCallerId = campaign.OutboundCallerId,
                TransferConf1 = campaign.TransferConf1,
                TransferConf2 = campaign.TransferConf2,
                TransferConf3 = campaign.TransferConf3,
                TransferConf4 = campaign.TransferConf4,
                TransferConf5 = campaign.TransferConf5,
                XferConfADtmf = campaign.XferConfADtmf,
                XferConfANumber = campaign.XferConfANumber,
                XferConfBDtmf = campaign.XferConfBDtmf,
                XferConfBNumber = campaign.XferConfBNumber,
                ContainerEntry = campaign.ContainerEntry,
                ManualDialFilter = campaign.ManualDialFilter,
                DispoCallUrl = campaign.DispoCallUrl,
                WebForm1 = campaign.WebForm1,
                WebForm2 = campaign.WebForm2,
                WebForm3 = campaign.WebForm3,
                WrapupSeconds = campaign.WrapupSeconds,
                DialStatuses = campaign.DialStatuses,
                LeadFilterId = campaign.LeadFilterId,
                DropCallTimer = campaign.DropCallTimer,
                SafeHarborMessage = campaign.SafeHarborMessage,
                MaxDropPercentage = campaign.MaxDropPercentage,
                IsActive = campaign.IsActive,
                Priority = campaign.Priority,
                LocationRestrictions = campaign.LocationRestrictions,
                EnableCallRecording = campaign.EnableCallRecording,
                CustomFields = campaign.CustomFields,
                RecyclingRules = campaign.RecyclingRules,
                EnableAutoRecycling = campaign.EnableAutoRecycling,
                MaxRecycleCount = campaign.MaxRecycleCount,
                StatsLastUpdated = campaign.StatsLastUpdated,
                TotalDialedToday = campaign.TotalDialedToday,
                TotalContactsToday = campaign.TotalContactsToday,
                CurrentDropRate = campaign.CurrentDropRate,
                CreatedAt = campaign.CreatedAt,
                UpdatedAt = campaign.UpdatedAt,
                CreatedBy = campaign.CreatedBy,
                UpdatedBy = campaign.UpdatedBy
            };

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(campaignDto);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving campaign with ID: {CampaignId}", id);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("An error occurred while retrieving the campaign");
            return response;
        }
    }

    [Function("CreateCampaign")]
    public async Task<HttpResponseData> CreateCampaign([HttpTrigger(AuthorizationLevel.Function, "post", Route = "campaigns")] HttpRequestData req)
    {
        try
        {
            _logger.LogInformation("Creating new campaign");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var createDto = JsonSerializer.Deserialize<CreateCampaignDto>(requestBody, _jsonOptions);

            if (createDto == null)
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Invalid request body");
                return badRequestResponse;
            }

            var campaign = new Campaign
            {
                // Core Campaign Properties
                Name = createDto.Name,
                Description = createDto.Description,
                DialMethod = createDto.DialMethod,
                DialingRatio = createDto.DialingRatio,
                ApplyRatioToIdleAgentsOnly = createDto.ApplyRatioToIdleAgentsOnly,
                AdaptiveMaxLevel = createDto.AdaptiveMaxLevel,
                DialTimeout = createDto.DialTimeout,
                MaxConcurrentCalls = createDto.MaxConcurrentCalls,
                
                // Dial Prefixes and Extensions
                DialPrefix = createDto.DialPrefix,
                ManualDialPrefix = createDto.ManualDialPrefix,
                ThreeWayDialPrefix = createDto.ThreeWayDialPrefix,
                ParkExtension = createDto.ParkExtension,
                ParkFileName = createDto.ParkFileName,
                VoicemailExtension = createDto.VoicemailExtension,
                AnsweringMachineExtension = createDto.AnsweringMachineExtension,
                
                // Agent Selection and Routing
                AvailableOnlyRatioTally = createDto.AvailableOnlyRatioTally,
                AvailableOnlyTallyThreshold = createDto.AvailableOnlyTallyThreshold,
                NextAgentCall = createDto.NextAgentCall,
                
                // Call Handling and Features
                AlternateNumberDialing = createDto.AlternateNumberDialing,
                ScheduledCallbacks = createDto.ScheduledCallbacks,
                AllowInbound = createDto.AllowInbound,
                ForceResetHopper = createDto.ForceResetHopper,
                HopperDuplicateCheck = createDto.HopperDuplicateCheck,
                GetCallLaunch = createDto.GetCallLaunch,
                
                // Time Restrictions
                TimeZone = createDto.TimeZone,
                CallStartTime = createDto.CallStartTime,
                CallEndTime = createDto.CallEndTime,
                AllowedDaysOfWeek = createDto.AllowedDaysOfWeek,
                RespectLeadTimeZone = createDto.RespectLeadTimeZone,
                
                // Call Attempt Configuration
                MinCallInterval = createDto.MinCallInterval,
                MaxCallAttempts = createDto.MaxCallAttempts,
                CallAttemptDelay = createDto.CallAttemptDelay,
                
                // Answering Machine Detection
                EnableAnsweringMachineDetection = createDto.EnableAnsweringMachineDetection,
                AnsweringMachineAction = createDto.AnsweringMachineAction,
                AnsweringMachineMessage = createDto.AnsweringMachineMessage,
                
                // Lead Processing
                LeadOrder = createDto.LeadOrder,
                RandomizeLeadOrder = createDto.RandomizeLeadOrder,
                LeadOrderSecondary = createDto.LeadOrderSecondary,
                HopperLevel = createDto.HopperLevel,
                
                // Transfer Configuration
                OutboundCallerId = createDto.OutboundCallerId,
                TransferConf1 = createDto.TransferConf1,
                TransferConf2 = createDto.TransferConf2,
                TransferConf3 = createDto.TransferConf3,
                TransferConf4 = createDto.TransferConf4,
                TransferConf5 = createDto.TransferConf5,
                XferConfADtmf = createDto.XferConfADtmf,
                XferConfANumber = createDto.XferConfANumber,
                XferConfBDtmf = createDto.XferConfBDtmf,
                XferConfBNumber = createDto.XferConfBNumber,
                
                // Advanced Features
                ContainerEntry = createDto.ContainerEntry,
                ManualDialFilter = createDto.ManualDialFilter,
                DispoCallUrl = createDto.DispoCallUrl,
                WebForm1 = createDto.WebForm1,
                WebForm2 = createDto.WebForm2,
                WebForm3 = createDto.WebForm3,
                WrapupSeconds = createDto.WrapupSeconds,
                DialStatuses = createDto.DialStatuses,
                LeadFilterId = createDto.LeadFilterId,
                DropCallTimer = createDto.DropCallTimer,
                SafeHarborMessage = createDto.SafeHarborMessage,
                MaxDropPercentage = createDto.MaxDropPercentage,
                
                // Campaign Settings
                IsActive = createDto.IsActive,
                Priority = createDto.Priority,
                LocationRestrictions = createDto.LocationRestrictions,
                EnableCallRecording = createDto.EnableCallRecording,
                CustomFields = createDto.CustomFields,
                
                // Lead Recycling
                RecyclingRules = createDto.RecyclingRules,
                EnableAutoRecycling = createDto.EnableAutoRecycling,
                MaxRecycleCount = createDto.MaxRecycleCount,
                
                // Audit fields
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System", // TODO: Get from authentication context
                UpdatedBy = "System"
            };

            _dbContext.Campaigns.Add(campaign);
            await _dbContext.SaveChangesAsync();

            var campaignDto = new CampaignDto
            {
                Id = campaign.Id,
                Name = campaign.Name,
                Description = campaign.Description,
                DialMethod = campaign.DialMethod,
                DialingRatio = campaign.DialingRatio,
                ApplyRatioToIdleAgentsOnly = campaign.ApplyRatioToIdleAgentsOnly,
                AdaptiveMaxLevel = campaign.AdaptiveMaxLevel,
                DialTimeout = campaign.DialTimeout,
                MaxConcurrentCalls = campaign.MaxConcurrentCalls,
                DialPrefix = campaign.DialPrefix,
                ManualDialPrefix = campaign.ManualDialPrefix,
                ThreeWayDialPrefix = campaign.ThreeWayDialPrefix,
                ParkExtension = campaign.ParkExtension,
                ParkFileName = campaign.ParkFileName,
                VoicemailExtension = campaign.VoicemailExtension,
                AnsweringMachineExtension = campaign.AnsweringMachineExtension,
                AvailableOnlyRatioTally = campaign.AvailableOnlyRatioTally,
                AvailableOnlyTallyThreshold = campaign.AvailableOnlyTallyThreshold,
                NextAgentCall = campaign.NextAgentCall,
                AlternateNumberDialing = campaign.AlternateNumberDialing,
                ScheduledCallbacks = campaign.ScheduledCallbacks,
                AllowInbound = campaign.AllowInbound,
                ForceResetHopper = campaign.ForceResetHopper,
                HopperDuplicateCheck = campaign.HopperDuplicateCheck,
                GetCallLaunch = campaign.GetCallLaunch,
                TimeZone = campaign.TimeZone,
                CallStartTime = campaign.CallStartTime,
                CallEndTime = campaign.CallEndTime,
                AllowedDaysOfWeek = campaign.AllowedDaysOfWeek,
                RespectLeadTimeZone = campaign.RespectLeadTimeZone,
                MinCallInterval = campaign.MinCallInterval,
                MaxCallAttempts = campaign.MaxCallAttempts,
                CallAttemptDelay = campaign.CallAttemptDelay,
                EnableAnsweringMachineDetection = campaign.EnableAnsweringMachineDetection,
                AnsweringMachineAction = campaign.AnsweringMachineAction,
                AnsweringMachineMessage = campaign.AnsweringMachineMessage,
                LeadOrder = campaign.LeadOrder,
                RandomizeLeadOrder = campaign.RandomizeLeadOrder,
                LeadOrderSecondary = campaign.LeadOrderSecondary,
                HopperLevel = campaign.HopperLevel,
                OutboundCallerId = campaign.OutboundCallerId,
                TransferConf1 = campaign.TransferConf1,
                TransferConf2 = campaign.TransferConf2,
                TransferConf3 = campaign.TransferConf3,
                TransferConf4 = campaign.TransferConf4,
                TransferConf5 = campaign.TransferConf5,
                XferConfADtmf = campaign.XferConfADtmf,
                XferConfANumber = campaign.XferConfANumber,
                XferConfBDtmf = campaign.XferConfBDtmf,
                XferConfBNumber = campaign.XferConfBNumber,
                ContainerEntry = campaign.ContainerEntry,
                ManualDialFilter = campaign.ManualDialFilter,
                DispoCallUrl = campaign.DispoCallUrl,
                WebForm1 = campaign.WebForm1,
                WebForm2 = campaign.WebForm2,
                WebForm3 = campaign.WebForm3,
                WrapupSeconds = campaign.WrapupSeconds,
                DialStatuses = campaign.DialStatuses,
                LeadFilterId = campaign.LeadFilterId,
                DropCallTimer = campaign.DropCallTimer,
                SafeHarborMessage = campaign.SafeHarborMessage,
                MaxDropPercentage = campaign.MaxDropPercentage,
                IsActive = campaign.IsActive,
                Priority = campaign.Priority,
                LocationRestrictions = campaign.LocationRestrictions,
                EnableCallRecording = campaign.EnableCallRecording,
                CustomFields = campaign.CustomFields,
                RecyclingRules = campaign.RecyclingRules,
                EnableAutoRecycling = campaign.EnableAutoRecycling,
                MaxRecycleCount = campaign.MaxRecycleCount,
                StatsLastUpdated = campaign.StatsLastUpdated,
                TotalDialedToday = campaign.TotalDialedToday,
                TotalContactsToday = campaign.TotalContactsToday,
                CurrentDropRate = campaign.CurrentDropRate,
                CreatedAt = campaign.CreatedAt,
                UpdatedAt = campaign.UpdatedAt,
                CreatedBy = campaign.CreatedBy,
                UpdatedBy = campaign.UpdatedBy
            };

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(campaignDto);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating campaign");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("An error occurred while creating the campaign");
            return response;
        }
    }

    [Function("UpdateCampaign")]
    public async Task<HttpResponseData> UpdateCampaign([HttpTrigger(AuthorizationLevel.Function, "put", Route = "campaigns/{id:int}")] HttpRequestData req, int id)
    {
        try
        {
            _logger.LogInformation("Updating campaign with ID: {CampaignId}", id);

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updateDto = JsonSerializer.Deserialize<UpdateCampaignDto>(requestBody, _jsonOptions);

            if (updateDto == null)
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Invalid request body");
                return badRequestResponse;
            }

            var campaign = await _dbContext.Campaigns
                .FirstOrDefaultAsync(c => c.Id == id);

            if (campaign == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"Campaign with ID {id} not found");
                return notFoundResponse;
            }

            // Update all campaign properties with comprehensive VICIdial field support
            campaign.Name = updateDto.Name;
            campaign.Description = updateDto.Description;
            campaign.DialMethod = updateDto.DialMethod;
            campaign.DialingRatio = updateDto.DialingRatio;
            campaign.ApplyRatioToIdleAgentsOnly = updateDto.ApplyRatioToIdleAgentsOnly;
            campaign.AdaptiveMaxLevel = updateDto.AdaptiveMaxLevel;
            campaign.DialTimeout = updateDto.DialTimeout;
            campaign.MaxConcurrentCalls = updateDto.MaxConcurrentCalls;
            campaign.DialPrefix = updateDto.DialPrefix;
            campaign.ManualDialPrefix = updateDto.ManualDialPrefix;
            campaign.ThreeWayDialPrefix = updateDto.ThreeWayDialPrefix;
            campaign.ParkExtension = updateDto.ParkExtension;
            campaign.ParkFileName = updateDto.ParkFileName;
            campaign.VoicemailExtension = updateDto.VoicemailExtension;
            campaign.AnsweringMachineExtension = updateDto.AnsweringMachineExtension;
            campaign.AvailableOnlyRatioTally = updateDto.AvailableOnlyRatioTally;
            campaign.AvailableOnlyTallyThreshold = updateDto.AvailableOnlyTallyThreshold;
            campaign.NextAgentCall = updateDto.NextAgentCall;
            campaign.AlternateNumberDialing = updateDto.AlternateNumberDialing;
            campaign.ScheduledCallbacks = updateDto.ScheduledCallbacks;
            campaign.AllowInbound = updateDto.AllowInbound;
            campaign.ForceResetHopper = updateDto.ForceResetHopper;
            campaign.HopperDuplicateCheck = updateDto.HopperDuplicateCheck;
            campaign.GetCallLaunch = updateDto.GetCallLaunch;
            campaign.TimeZone = updateDto.TimeZone;
            campaign.CallStartTime = updateDto.CallStartTime;
            campaign.CallEndTime = updateDto.CallEndTime;
            campaign.AllowedDaysOfWeek = updateDto.AllowedDaysOfWeek;
            campaign.RespectLeadTimeZone = updateDto.RespectLeadTimeZone;
            campaign.MinCallInterval = updateDto.MinCallInterval;
            campaign.MaxCallAttempts = updateDto.MaxCallAttempts;
            campaign.CallAttemptDelay = updateDto.CallAttemptDelay;
            campaign.EnableAnsweringMachineDetection = updateDto.EnableAnsweringMachineDetection;
            campaign.AnsweringMachineAction = updateDto.AnsweringMachineAction;
            campaign.AnsweringMachineMessage = updateDto.AnsweringMachineMessage;
            campaign.LeadOrder = updateDto.LeadOrder;
            campaign.RandomizeLeadOrder = updateDto.RandomizeLeadOrder;
            campaign.LeadOrderSecondary = updateDto.LeadOrderSecondary;
            campaign.HopperLevel = updateDto.HopperLevel;
            campaign.OutboundCallerId = updateDto.OutboundCallerId;
            campaign.TransferConf1 = updateDto.TransferConf1;
            campaign.TransferConf2 = updateDto.TransferConf2;
            campaign.TransferConf3 = updateDto.TransferConf3;
            campaign.TransferConf4 = updateDto.TransferConf4;
            campaign.TransferConf5 = updateDto.TransferConf5;
            campaign.XferConfADtmf = updateDto.XferConfADtmf;
            campaign.XferConfANumber = updateDto.XferConfANumber;
            campaign.XferConfBDtmf = updateDto.XferConfBDtmf;
            campaign.XferConfBNumber = updateDto.XferConfBNumber;
            campaign.ContainerEntry = updateDto.ContainerEntry;
            campaign.ManualDialFilter = updateDto.ManualDialFilter;
            campaign.DispoCallUrl = updateDto.DispoCallUrl;
            campaign.WebForm1 = updateDto.WebForm1;
            campaign.WebForm2 = updateDto.WebForm2;
            campaign.WebForm3 = updateDto.WebForm3;
            campaign.WrapupSeconds = updateDto.WrapupSeconds;
            campaign.DialStatuses = updateDto.DialStatuses;
            campaign.LeadFilterId = updateDto.LeadFilterId;
            campaign.DropCallTimer = updateDto.DropCallTimer;
            campaign.SafeHarborMessage = updateDto.SafeHarborMessage;
            campaign.MaxDropPercentage = updateDto.MaxDropPercentage;
            campaign.IsActive = updateDto.IsActive;
            campaign.Priority = updateDto.Priority;
            campaign.LocationRestrictions = updateDto.LocationRestrictions;
            campaign.EnableCallRecording = updateDto.EnableCallRecording;
            campaign.CustomFields = updateDto.CustomFields;
            campaign.RecyclingRules = updateDto.RecyclingRules;
            campaign.EnableAutoRecycling = updateDto.EnableAutoRecycling;
            campaign.MaxRecycleCount = updateDto.MaxRecycleCount;
            campaign.UpdatedAt = DateTime.UtcNow;
            campaign.UpdatedBy = "System"; // TODO: Get from authentication context

            await _dbContext.SaveChangesAsync();

            var campaignDto = new CampaignDto
            {
                Id = campaign.Id,
                Name = campaign.Name,
                Description = campaign.Description,
                DialMethod = campaign.DialMethod,
                DialingRatio = campaign.DialingRatio,
                ApplyRatioToIdleAgentsOnly = campaign.ApplyRatioToIdleAgentsOnly,
                AdaptiveMaxLevel = campaign.AdaptiveMaxLevel,
                DialTimeout = campaign.DialTimeout,
                MaxConcurrentCalls = campaign.MaxConcurrentCalls,
                DialPrefix = campaign.DialPrefix,
                ManualDialPrefix = campaign.ManualDialPrefix,
                ThreeWayDialPrefix = campaign.ThreeWayDialPrefix,
                ParkExtension = campaign.ParkExtension,
                ParkFileName = campaign.ParkFileName,
                VoicemailExtension = campaign.VoicemailExtension,
                AnsweringMachineExtension = campaign.AnsweringMachineExtension,
                AvailableOnlyRatioTally = campaign.AvailableOnlyRatioTally,
                AvailableOnlyTallyThreshold = campaign.AvailableOnlyTallyThreshold,
                NextAgentCall = campaign.NextAgentCall,
                AlternateNumberDialing = campaign.AlternateNumberDialing,
                ScheduledCallbacks = campaign.ScheduledCallbacks,
                AllowInbound = campaign.AllowInbound,
                ForceResetHopper = campaign.ForceResetHopper,
                HopperDuplicateCheck = campaign.HopperDuplicateCheck,
                GetCallLaunch = campaign.GetCallLaunch,
                TimeZone = campaign.TimeZone,
                CallStartTime = campaign.CallStartTime,
                CallEndTime = campaign.CallEndTime,
                AllowedDaysOfWeek = campaign.AllowedDaysOfWeek,
                RespectLeadTimeZone = campaign.RespectLeadTimeZone,
                MinCallInterval = campaign.MinCallInterval,
                MaxCallAttempts = campaign.MaxCallAttempts,
                CallAttemptDelay = campaign.CallAttemptDelay,
                EnableAnsweringMachineDetection = campaign.EnableAnsweringMachineDetection,
                AnsweringMachineAction = campaign.AnsweringMachineAction,
                AnsweringMachineMessage = campaign.AnsweringMachineMessage,
                LeadOrder = campaign.LeadOrder,
                RandomizeLeadOrder = campaign.RandomizeLeadOrder,
                LeadOrderSecondary = campaign.LeadOrderSecondary,
                HopperLevel = campaign.HopperLevel,
                OutboundCallerId = campaign.OutboundCallerId,
                TransferConf1 = campaign.TransferConf1,
                TransferConf2 = campaign.TransferConf2,
                TransferConf3 = campaign.TransferConf3,
                TransferConf4 = campaign.TransferConf4,
                TransferConf5 = campaign.TransferConf5,
                XferConfADtmf = campaign.XferConfADtmf,
                XferConfANumber = campaign.XferConfANumber,
                XferConfBDtmf = campaign.XferConfBDtmf,
                XferConfBNumber = campaign.XferConfBNumber,
                ContainerEntry = campaign.ContainerEntry,
                ManualDialFilter = campaign.ManualDialFilter,
                DispoCallUrl = campaign.DispoCallUrl,
                WebForm1 = campaign.WebForm1,
                WebForm2 = campaign.WebForm2,
                WebForm3 = campaign.WebForm3,
                WrapupSeconds = campaign.WrapupSeconds,
                DialStatuses = campaign.DialStatuses,
                LeadFilterId = campaign.LeadFilterId,
                DropCallTimer = campaign.DropCallTimer,
                SafeHarborMessage = campaign.SafeHarborMessage,
                MaxDropPercentage = campaign.MaxDropPercentage,
                IsActive = campaign.IsActive,
                Priority = campaign.Priority,
                LocationRestrictions = campaign.LocationRestrictions,
                EnableCallRecording = campaign.EnableCallRecording,
                CustomFields = campaign.CustomFields,
                RecyclingRules = campaign.RecyclingRules,
                EnableAutoRecycling = campaign.EnableAutoRecycling,
                MaxRecycleCount = campaign.MaxRecycleCount,
                StatsLastUpdated = campaign.StatsLastUpdated,
                TotalDialedToday = campaign.TotalDialedToday,
                TotalContactsToday = campaign.TotalContactsToday,
                CurrentDropRate = campaign.CurrentDropRate,
                CreatedAt = campaign.CreatedAt,
                UpdatedAt = campaign.UpdatedAt,
                CreatedBy = campaign.CreatedBy,
                UpdatedBy = campaign.UpdatedBy
            };

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(campaignDto);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating campaign with ID: {CampaignId}", id);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("An error occurred while updating the campaign");
            return response;
        }
    }

    [Function("DeleteCampaign")]
    public async Task<HttpResponseData> DeleteCampaign([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "campaigns/{id:int}")] HttpRequestData req, int id)
    {
        try
        {
            _logger.LogInformation("Deleting campaign with ID: {CampaignId}", id);

            var campaign = await _dbContext.Campaigns
                .FirstOrDefaultAsync(c => c.Id == id);

            if (campaign == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"Campaign with ID {id} not found");
                return notFoundResponse;
            }

            _dbContext.Campaigns.Remove(campaign);
            await _dbContext.SaveChangesAsync();

            var response = req.CreateResponse(HttpStatusCode.NoContent);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting campaign with ID: {CampaignId}", id);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("An error occurred while deleting the campaign");
            return response;
        }
    }

    [Function("GetCampaignLists")]
    public async Task<HttpResponseData> GetCampaignLists([HttpTrigger(AuthorizationLevel.Function, "get", Route = "campaigns/{id:int}/lists")] HttpRequestData req, int id)
    {
        try
        {
            _logger.LogInformation("Retrieving lists for campaign with ID: {CampaignId}", id);

            var campaign = await _dbContext.Campaigns
                .FirstOrDefaultAsync(c => c.Id == id);

            if (campaign == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"Campaign with ID {id} not found");
                return notFoundResponse;
            }

            var lists = await _dbContext.CampaignLists
                .Where(cl => cl.CampaignId == id)
                .Include(cl => cl.List)
                .Select(cl => new ListSummaryDto
                {
                    Id = cl.List.Id,
                    Name = cl.List.Name,
                    Description = cl.List.Description,
                    IsActive = cl.List.IsActive,
                    CreatedAt = cl.List.CreatedAt,
                    UpdatedAt = cl.List.UpdatedAt,
                    TotalLeads = 0, // Will be calculated separately
                    CalledLeads = 0,
                    ContactedLeads = 0
                })
                .ToListAsync();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(lists);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lists for campaign with ID: {CampaignId}", id);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("An error occurred while retrieving campaign lists");
            return response;
        }
    }

    [Function("ToggleCampaignStatus")]
    public async Task<HttpResponseData> ToggleCampaignStatus([HttpTrigger(AuthorizationLevel.Function, "post", Route = "campaigns/{id:int}/toggle-status")] HttpRequestData req, int id)
    {
        try
        {
            _logger.LogInformation("Toggling status for campaign with ID: {CampaignId}", id);

            var campaign = await _dbContext.Campaigns
                .FirstOrDefaultAsync(c => c.Id == id);

            if (campaign == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"Campaign with ID {id} not found");
                return notFoundResponse;
            }

            campaign.IsActive = !campaign.IsActive;
            campaign.UpdatedAt = DateTime.UtcNow;
            campaign.UpdatedBy = "System"; // TODO: Get from authentication context

            await _dbContext.SaveChangesAsync();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new { id = campaign.Id, isActive = campaign.IsActive });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling status for campaign with ID: {CampaignId}", id);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("An error occurred while toggling campaign status");
            return response;
        }
    }
}