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
/// Azure Functions for managing lists
/// Provides CRUD operations and list-specific functionality with comprehensive VICIdial feature support
/// </summary>
public class ListFunctions
{
    private readonly ProDialerDbContext _dbContext;
    private readonly ILogger<ListFunctions> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ListFunctions(ProDialerDbContext dbContext, ILogger<ListFunctions> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    [Function("GetLists")]
    public async Task<HttpResponseData> GetLists([HttpTrigger(AuthorizationLevel.Function, "get", Route = "lists")] HttpRequestData req)
    {
        try
        {
            _logger.LogInformation("Retrieving lists");

            var lists = await _dbContext.Lists
                .OrderBy(l => l.Name)
                .Select(l => new ListSummaryDto
                {
                    Id = l.Id,
                    Name = l.Name,
                    Description = l.Description,
                    IsActive = l.IsActive,
                    Priority = l.Priority,
                    Source = l.Source,
                    CreatedAt = l.CreatedAt,
                    UpdatedAt = l.UpdatedAt,
                    ListMixRatio = l.ListMixRatio,
                    DuplicateCheckMethod = l.DuplicateCheckMethod,
                    ResetsToday = l.ResetsToday,
                    LastResetAt = l.LastResetAt,
                    ExpirationDate = l.ExpirationDate,
                    TotalLeads = 0, // Will be calculated separately for performance
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
            _logger.LogError(ex, "Error retrieving lists");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("An error occurred while retrieving lists");
            return response;
        }
    }

    [Function("GetListById")]
    public async Task<HttpResponseData> GetListById([HttpTrigger(AuthorizationLevel.Function, "get", Route = "lists/{id:int}")] HttpRequestData req, int id)
    {
        try
        {
            _logger.LogInformation("Retrieving list with ID: {ListId}", id);

            var list = await _dbContext.Lists
                .FirstOrDefaultAsync(l => l.Id == id);

            if (list == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"List with ID {id} not found");
                return notFoundResponse;
            }

            var listDto = new ListDto
            {
                Id = list.Id,
                Name = list.Name,
                Description = list.Description,
                Priority = list.Priority,
                CallStrategy = list.CallStrategy,
                MaxCallAttempts = list.MaxCallAttempts,
                MinCallInterval = list.MinCallInterval,
                TimeZoneOverride = list.TimeZoneOverride,
                CallStartTimeOverride = list.CallStartTimeOverride,
                CallEndTimeOverride = list.CallEndTimeOverride,
                AllowedDaysOverride = list.AllowedDaysOverride,
                IsActive = list.IsActive,
                Source = list.Source,
                SourceReference = list.SourceReference,
                CustomFieldsSchema = list.CustomFieldsSchema,
                Tags = list.Tags,
                ScriptId = list.ScriptId,
                AgentScriptOverride = list.AgentScriptOverride,
                CampaignCallerIdOverride = list.CampaignCallerIdOverride,
                ListMixRatio = list.ListMixRatio,
                DuplicateCheckMethod = list.DuplicateCheckMethod,
                CustomFieldsCopy = list.CustomFieldsCopy,
                CustomFieldsModify = list.CustomFieldsModify,
                ResetLeadCalledCount = list.ResetLeadCalledCount,
                PhoneValidationSettings = list.PhoneValidationSettings,
                ImportExportLog = list.ImportExportLog,
                PerformanceMetrics = list.PerformanceMetrics,
                AnsweringMachineMessage = list.AnsweringMachineMessage,
                DropInGroup = list.DropInGroup,
                WebFormAddress = list.WebFormAddress,
                WebFormAddress2 = list.WebFormAddress2,
                WebFormAddress3 = list.WebFormAddress3,
                ResetTime = list.ResetTime,
                TimezoneMethod = list.TimezoneMethod,
                LocalCallTime = list.LocalCallTime,
                ExpirationDate = list.ExpirationDate,
                OutboundCallerId = list.OutboundCallerId,
                TransferConf1 = list.TransferConf1,
                TransferConf2 = list.TransferConf2,
                TransferConf3 = list.TransferConf3,
                TransferConf4 = list.TransferConf4,
                TransferConf5 = list.TransferConf5,
                ResetsToday = list.ResetsToday,
                LastResetAt = list.LastResetAt,
                TotalLeads = 0, // Will be calculated separately
                CalledLeads = 0,
                ContactedLeads = 0,
                CreatedAt = list.CreatedAt,
                UpdatedAt = list.UpdatedAt,
                CreatedBy = list.CreatedBy,
                UpdatedBy = list.UpdatedBy
            };

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(listDto);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving list with ID: {ListId}", id);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("An error occurred while retrieving the list");
            return response;
        }
    }

    [Function("CreateList")]
    public async Task<HttpResponseData> CreateList([HttpTrigger(AuthorizationLevel.Function, "post", Route = "lists")] HttpRequestData req)
    {
        try
        {
            _logger.LogInformation("Creating new list");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var createDto = JsonSerializer.Deserialize<CreateListDto>(requestBody, _jsonOptions);

            if (createDto == null)
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Invalid request body");
                return badRequestResponse;
            }

            var list = new List
            {
                Name = createDto.Name,
                Description = createDto.Description,
                Priority = createDto.Priority,
                CallStrategy = createDto.CallStrategy,
                MaxCallAttempts = createDto.MaxCallAttempts,
                MinCallInterval = createDto.MinCallInterval,
                TimeZoneOverride = createDto.TimeZoneOverride,
                CallStartTimeOverride = createDto.CallStartTimeOverride,
                CallEndTimeOverride = createDto.CallEndTimeOverride,
                AllowedDaysOverride = createDto.AllowedDaysOverride,
                IsActive = createDto.IsActive,
                Source = createDto.Source,
                SourceReference = createDto.SourceReference,
                CustomFieldsSchema = createDto.CustomFieldsSchema,
                Tags = createDto.Tags,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System", // TODO: Get from authentication context
                UpdatedBy = "System"
            };

            _dbContext.Lists.Add(list);
            await _dbContext.SaveChangesAsync();

            var listDto = new ListDto
            {
                Id = list.Id,
                Name = list.Name,
                Description = list.Description,
                Priority = list.Priority,
                CallStrategy = list.CallStrategy,
                MaxCallAttempts = list.MaxCallAttempts,
                MinCallInterval = list.MinCallInterval,
                TimeZoneOverride = list.TimeZoneOverride,
                CallStartTimeOverride = list.CallStartTimeOverride,
                CallEndTimeOverride = list.CallEndTimeOverride,
                AllowedDaysOverride = list.AllowedDaysOverride,
                IsActive = list.IsActive,
                Source = list.Source,
                SourceReference = list.SourceReference,
                CustomFieldsSchema = list.CustomFieldsSchema,
                Tags = list.Tags,
                CreatedAt = list.CreatedAt,
                UpdatedAt = list.UpdatedAt,
                CreatedBy = list.CreatedBy,
                UpdatedBy = list.UpdatedBy,
                TotalLeads = 0,
                CalledLeads = 0,
                ContactedLeads = 0
            };

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(listDto);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating list");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("An error occurred while creating the list");
            return response;
        }
    }

    [Function("UpdateList")]
    public async Task<HttpResponseData> UpdateList([HttpTrigger(AuthorizationLevel.Function, "put", Route = "lists/{id:int}")] HttpRequestData req, int id)
    {
        try
        {
            _logger.LogInformation("Updating list with ID: {ListId}", id);

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updateDto = JsonSerializer.Deserialize<UpdateListDto>(requestBody);

            if (updateDto == null)
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Invalid request body");
                return badRequestResponse;
            }

            var list = await _dbContext.Lists
                .FirstOrDefaultAsync(l => l.Id == id);

            if (list == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"List with ID {id} not found");
                return notFoundResponse;
            }

            // Update list properties
            list.Name = updateDto.Name;
            list.Description = updateDto.Description;
            list.Priority = updateDto.Priority;
            list.CallStrategy = updateDto.CallStrategy;
            list.MaxCallAttempts = updateDto.MaxCallAttempts;
            list.MinCallInterval = updateDto.MinCallInterval;
            list.TimeZoneOverride = updateDto.TimeZoneOverride;
            list.CallStartTimeOverride = updateDto.CallStartTimeOverride;
            list.CallEndTimeOverride = updateDto.CallEndTimeOverride;
            list.AllowedDaysOverride = updateDto.AllowedDaysOverride;
            list.IsActive = updateDto.IsActive;
            list.Source = updateDto.Source;
            list.SourceReference = updateDto.SourceReference;
            list.CustomFieldsSchema = updateDto.CustomFieldsSchema;
            list.Tags = updateDto.Tags;
            list.UpdatedAt = DateTime.UtcNow;
            list.UpdatedBy = "System"; // TODO: Get from authentication context

            await _dbContext.SaveChangesAsync();

            var listDto = new ListDto
            {
                Id = list.Id,
                Name = list.Name,
                Description = list.Description,
                Priority = list.Priority,
                CallStrategy = list.CallStrategy,
                MaxCallAttempts = list.MaxCallAttempts,
                MinCallInterval = list.MinCallInterval,
                TimeZoneOverride = list.TimeZoneOverride,
                CallStartTimeOverride = list.CallStartTimeOverride,
                CallEndTimeOverride = list.CallEndTimeOverride,
                AllowedDaysOverride = list.AllowedDaysOverride,
                IsActive = list.IsActive,
                Source = list.Source,
                SourceReference = list.SourceReference,
                CustomFieldsSchema = list.CustomFieldsSchema,
                Tags = list.Tags,
                CreatedAt = list.CreatedAt,
                UpdatedAt = list.UpdatedAt,
                CreatedBy = list.CreatedBy,
                UpdatedBy = list.UpdatedBy,
                TotalLeads = 0,
                CalledLeads = 0,
                ContactedLeads = 0
            };

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(listDto);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating list with ID: {ListId}", id);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("An error occurred while updating the list");
            return response;
        }
    }

    [Function("DeleteList")]
    public async Task<HttpResponseData> DeleteList([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "lists/{id:int}")] HttpRequestData req, int id)
    {
        try
        {
            _logger.LogInformation("Deleting list with ID: {ListId}", id);

            var list = await _dbContext.Lists
                .FirstOrDefaultAsync(l => l.Id == id);

            if (list == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"List with ID {id} not found");
                return notFoundResponse;
            }

            _dbContext.Lists.Remove(list);
            await _dbContext.SaveChangesAsync();

            var response = req.CreateResponse(HttpStatusCode.NoContent);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting list with ID: {ListId}", id);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("An error occurred while deleting the list");
            return response;
        }
    }

    [Function("GetListLeads")]
    public async Task<HttpResponseData> GetListLeads([HttpTrigger(AuthorizationLevel.Function, "get", Route = "lists/{id:int}/leads")] HttpRequestData req, int id)
    {
        try
        {
            _logger.LogInformation("Retrieving leads for list with ID: {ListId}", id);

            var list = await _dbContext.Lists
                .FirstOrDefaultAsync(l => l.Id == id);

            if (list == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"List with ID {id} not found");
                return notFoundResponse;
            }

            var leads = await _dbContext.Leads
                .Where(l => l.ListId == id)
                .OrderBy(l => l.Id)
                .Select(l => new LeadSummaryDto
                {
                    Id = l.Id,
                    FirstName = l.FirstName,
                    LastName = l.LastName,
                    FullName = l.FullName,
                    PrimaryPhone = l.PrimaryPhone,
                    PrimaryEmail = l.PrimaryEmail,
                    Status = l.Status,
                    Priority = l.Priority,
                    CallCount = l.CallAttempts,
                    LastCallAttempt = l.LastCalledAt,
                    Disposition = l.Disposition,
                    CreatedAt = l.CreatedAt
                })
                .ToListAsync();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(leads);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leads for list with ID: {ListId}", id);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("An error occurred while retrieving list leads");
            return response;
        }
    }
}
