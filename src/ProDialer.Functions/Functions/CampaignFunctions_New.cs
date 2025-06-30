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
/// Provides CRUD operations and campaign-specific functionality
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
                    IsActive = c.IsActive,
                    DialingRatio = c.DialingRatio,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    TotalLeads = 0, // Will be calculated separately for performance
                    CalledLeads = 0,
                    ContactedLeads = 0
                })
                .ToListAsync();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(campaigns, _jsonOptions);
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
                DialingRatio = campaign.DialingRatio,
                ApplyRatioToIdleAgentsOnly = campaign.ApplyRatioToIdleAgentsOnly,
                MaxConcurrentCalls = campaign.MaxConcurrentCalls,
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
                IsActive = campaign.IsActive,
                Priority = campaign.Priority,
                LocationRestrictions = campaign.LocationRestrictions,
                EnableCallRecording = campaign.EnableCallRecording,
                CustomFields = campaign.CustomFields,
                CreatedAt = campaign.CreatedAt,
                UpdatedAt = campaign.UpdatedAt
            };

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(campaignDto, _jsonOptions);
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
                Name = createDto.Name,
                Description = createDto.Description,
                DialingRatio = createDto.DialingRatio,
                ApplyRatioToIdleAgentsOnly = createDto.ApplyRatioToIdleAgentsOnly,
                MaxConcurrentCalls = createDto.MaxConcurrentCalls,
                TimeZone = createDto.TimeZone ?? "UTC",
                CallStartTime = createDto.CallStartTime ?? "08:00",
                CallEndTime = createDto.CallEndTime ?? "20:00",
                AllowedDaysOfWeek = createDto.AllowedDaysOfWeek ?? "Monday,Tuesday,Wednesday,Thursday,Friday",
                RespectLeadTimeZone = createDto.RespectLeadTimeZone,
                MinCallInterval = createDto.MinCallInterval,
                MaxCallAttempts = createDto.MaxCallAttempts,
                CallAttemptDelay = createDto.CallAttemptDelay,
                EnableAnsweringMachineDetection = createDto.EnableAnsweringMachineDetection,
                AnsweringMachineAction = createDto.AnsweringMachineAction ?? "Hangup",
                AnsweringMachineMessage = createDto.AnsweringMachineMessage,
                IsActive = createDto.IsActive,
                Priority = createDto.Priority,
                LocationRestrictions = createDto.LocationRestrictions,
                EnableCallRecording = createDto.EnableCallRecording,
                CustomFields = createDto.CustomFields,
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
                DialingRatio = campaign.DialingRatio,
                ApplyRatioToIdleAgentsOnly = campaign.ApplyRatioToIdleAgentsOnly,
                MaxConcurrentCalls = campaign.MaxConcurrentCalls,
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
                IsActive = campaign.IsActive,
                Priority = campaign.Priority,
                LocationRestrictions = campaign.LocationRestrictions,
                EnableCallRecording = campaign.EnableCallRecording,
                CustomFields = campaign.CustomFields,
                CreatedAt = campaign.CreatedAt,
                UpdatedAt = campaign.UpdatedAt
            };

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(campaignDto, _jsonOptions);
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

            // Update campaign properties
            campaign.Name = updateDto.Name;
            campaign.Description = updateDto.Description;
            campaign.DialingRatio = updateDto.DialingRatio;
            campaign.ApplyRatioToIdleAgentsOnly = updateDto.ApplyRatioToIdleAgentsOnly;
            campaign.MaxConcurrentCalls = updateDto.MaxConcurrentCalls;
            campaign.TimeZone = updateDto.TimeZone ?? campaign.TimeZone;
            campaign.CallStartTime = updateDto.CallStartTime ?? campaign.CallStartTime;
            campaign.CallEndTime = updateDto.CallEndTime ?? campaign.CallEndTime;
            campaign.AllowedDaysOfWeek = updateDto.AllowedDaysOfWeek ?? campaign.AllowedDaysOfWeek;
            campaign.RespectLeadTimeZone = updateDto.RespectLeadTimeZone;
            campaign.MinCallInterval = updateDto.MinCallInterval;
            campaign.MaxCallAttempts = updateDto.MaxCallAttempts;
            campaign.CallAttemptDelay = updateDto.CallAttemptDelay;
            campaign.EnableAnsweringMachineDetection = updateDto.EnableAnsweringMachineDetection;
            campaign.AnsweringMachineAction = updateDto.AnsweringMachineAction ?? campaign.AnsweringMachineAction;
            campaign.AnsweringMachineMessage = updateDto.AnsweringMachineMessage;
            campaign.IsActive = updateDto.IsActive;
            campaign.Priority = updateDto.Priority;
            campaign.LocationRestrictions = updateDto.LocationRestrictions;
            campaign.EnableCallRecording = updateDto.EnableCallRecording;
            campaign.CustomFields = updateDto.CustomFields;
            campaign.UpdatedAt = DateTime.UtcNow;
            campaign.UpdatedBy = "System"; // TODO: Get from authentication context

            await _dbContext.SaveChangesAsync();

            var campaignDto = new CampaignDto
            {
                Id = campaign.Id,
                Name = campaign.Name,
                Description = campaign.Description,
                DialingRatio = campaign.DialingRatio,
                ApplyRatioToIdleAgentsOnly = campaign.ApplyRatioToIdleAgentsOnly,
                MaxConcurrentCalls = campaign.MaxConcurrentCalls,
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
                IsActive = campaign.IsActive,
                Priority = campaign.Priority,
                LocationRestrictions = campaign.LocationRestrictions,
                EnableCallRecording = campaign.EnableCallRecording,
                CustomFields = campaign.CustomFields,
                CreatedAt = campaign.CreatedAt,
                UpdatedAt = campaign.UpdatedAt
            };

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(campaignDto, _jsonOptions);
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
            await response.WriteAsJsonAsync(lists, _jsonOptions);
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
            await response.WriteAsJsonAsync(new { id = campaign.Id, isActive = campaign.IsActive }, _jsonOptions);
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
