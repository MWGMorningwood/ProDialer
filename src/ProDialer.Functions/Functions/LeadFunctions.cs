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
/// Azure Functions for managing leads
/// Provides CRUD operations and lead-specific functionality
/// </summary>
public class LeadFunctions
{
    private readonly ProDialerDbContext _dbContext;
    private readonly ILogger<LeadFunctions> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public LeadFunctions(ProDialerDbContext dbContext, ILogger<LeadFunctions> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    [Function("GetLeads")]
    public async Task<HttpResponseData> GetLeads([HttpTrigger(AuthorizationLevel.Function, "get", Route = "leads")] HttpRequestData req)
    {
        try
        {
            _logger.LogInformation("Retrieving leads");

            // Parse query parameters
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            var listIdParam = query["listId"];
            var page = int.TryParse(query["page"], out var p) ? p : 1;
            var pageSize = int.TryParse(query["pageSize"], out var ps) ? Math.Min(ps, 1000) : 50;
            var status = query["status"];
            var search = query["search"];

            var leadsQuery = _dbContext.Leads.AsQueryable();

            // Filter by list if specified
            if (int.TryParse(listIdParam, out var listId))
            {
                leadsQuery = leadsQuery.Where(l => l.ListId == listId);
            }

            // Filter by status if specified
            if (!string.IsNullOrEmpty(status))
            {
                leadsQuery = leadsQuery.Where(l => l.Status == status);
            }

            // Search filter
            if (!string.IsNullOrEmpty(search))
            {
                leadsQuery = leadsQuery.Where(l => 
                    (l.FirstName != null && l.FirstName.Contains(search)) ||
                    (l.LastName != null && l.LastName.Contains(search)) ||
                    (l.FullName != null && l.FullName.Contains(search)) ||
                    (l.Company != null && l.Company.Contains(search)) ||
                    l.PrimaryPhone.Contains(search) ||
                    (l.PrimaryEmail != null && l.PrimaryEmail.Contains(search)));
            }

            var totalCount = await leadsQuery.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var leads = await leadsQuery
                .OrderBy(l => l.Priority)
                .ThenBy(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new LeadSummaryDto
                {
                    Id = l.Id,
                    ListId = l.ListId,
                    FirstName = l.FirstName,
                    LastName = l.LastName,
                    FullName = l.FullName,
                    Company = l.Company,
                    PrimaryPhone = l.PrimaryPhone,
                    PrimaryEmail = l.PrimaryEmail,
                    Status = l.Status,
                    User = l.User,
                    Priority = l.Priority,
                    Rank = l.Rank,
                    CallCount = l.CallAttempts,
                    CalledCount = l.CalledCount,
                    QualityScore = l.QualityScore,
                    LifecycleStage = l.LifecycleStage,
                    LastCallAttempt = l.LastCalledAt,
                    ScheduledCallbackAt = l.ScheduledCallbackAt,
                    Disposition = l.Disposition,
                    Owner = l.Owner,
                    IsExcluded = l.IsExcluded,
                    HasOptedOut = l.HasOptedOut,
                    CreatedAt = l.CreatedAt,
                    UpdatedAt = l.UpdatedAt
                })
                .ToListAsync();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                leads,
                pagination = new
                {
                    currentPage = page,
                    pageSize,
                    totalCount,
                    totalPages
                }
            });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leads");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }

    [Function("GetLead")]
    public async Task<HttpResponseData> GetLead([HttpTrigger(AuthorizationLevel.Function, "get", Route = "leads/{id:int}")] HttpRequestData req, int id)
    {
        try
        {
            _logger.LogInformation("Retrieving lead {LeadId}", id);

            var lead = await _dbContext.Leads
                .Include(l => l.List)
                .Include(l => l.CallLogs)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lead == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync("Lead not found");
                return notFoundResponse;
            }

            var leadDto = new LeadDetailDto
            {
                Id = lead.Id,
                ListId = lead.ListId,
                ListName = lead.List?.Name ?? "Unknown",
                FirstName = lead.FirstName,
                LastName = lead.LastName,
                FullName = lead.FullName,
                Company = lead.Company,
                Title = lead.Title,
                MiddleInitial = lead.MiddleInitial,
                PrimaryPhone = lead.PrimaryPhone,
                PhoneCode = lead.PhoneCode,
                PhoneNumberRaw = lead.PhoneNumberRaw,
                PhoneValidationStatus = lead.PhoneValidationStatus,
                PhoneCarrier = lead.PhoneCarrier,
                PhoneType = lead.PhoneType,
                SecondaryPhone = lead.SecondaryPhone,
                MobilePhone = lead.MobilePhone,
                WorkPhone = lead.WorkPhone,
                HomePhone = lead.HomePhone,
                AlternatePhones = lead.AlternatePhones,
                VendorLeadCode = lead.VendorLeadCode,
                SourceId = lead.SourceId,
                PrimaryEmail = lead.PrimaryEmail,
                SecondaryEmail = lead.SecondaryEmail,
                AddressLine1 = lead.AddressLine1,
                AddressLine2 = lead.AddressLine2,
                AddressLine3 = lead.AddressLine3,
                City = lead.City,
                State = lead.State,
                Province = lead.Province,
                PostalCode = lead.PostalCode,
                Country = lead.Country,
                TimeZone = lead.TimeZone,
                Priority = lead.Priority,
                Rank = lead.Rank,
                QualityScore = lead.QualityScore,
                LifecycleStage = lead.LifecycleStage,
                Owner = lead.Owner,
                EntryListId = lead.EntryListId,
                GmtOffset = lead.GmtOffset,
                GmtOffsetNow = lead.GmtOffsetNow,
                SecurityPhrase = lead.SecurityPhrase,
                ComplianceFlags = lead.ComplianceFlags,
                CallbackAppointment = lead.CallbackAppointment,
                Status = lead.Status,
                User = lead.User,
                CallCount = lead.CallAttempts,
                CalledCount = lead.CalledCount,
                RecycleCount = lead.RecycleCount,
                LastLocalCallTime = lead.LastLocalCallTime,
                CalledSinceLastReset = lead.CalledSinceLastReset,
                LastCallAttempt = lead.LastCalledAt,
                LastContactedAt = lead.LastContactedAt,
                NextCallAt = lead.NextCallAt,
                ScheduledCallbackAt = lead.ScheduledCallbackAt,
                LastRecycledAt = lead.LastRecycledAt,
                LastHandlerAgent = lead.LastHandlerAgent,
                LastCallOutcome = lead.LastCallOutcome,
                Disposition = lead.Disposition,
                Gender = lead.Gender,
                DateOfBirth = lead.DateOfBirth,
                MaritalStatus = lead.MaritalStatus,
                Income = lead.Income,
                Education = lead.Education,
                Interests = lead.Interests,
                Source = lead.Source,
                SourceCampaign = lead.SourceCampaign,
                LeadValue = lead.LeadValue,
                ExpectedRevenue = lead.ExpectedRevenue,
                Tags = lead.Tags,
                CustomFields = lead.CustomFields,
                ConversionTracking = lead.ConversionTracking,
                InteractionHistory = lead.InteractionHistory,
                ScoringFactors = lead.ScoringFactors,
                AttributionData = lead.AttributionData,
                Notes = lead.Notes,
                IsExcluded = lead.IsExcluded,
                ExclusionReason = lead.ExclusionReason,
                ExcludedAt = lead.ExcludedAt,
                HasOptedOut = lead.HasOptedOut,
                OptedOutAt = lead.OptedOutAt,
                CreatedAt = lead.CreatedAt,
                UpdatedAt = lead.UpdatedAt,
                ModifyDate = lead.ModifyDate,
                CallHistory = lead.CallLogs?.Select(cl => new CallLogSummaryDto
                {
                    Id = cl.Id,
                    CampaignId = cl.CampaignId,
                    LeadId = cl.LeadId,
                    PhoneNumber = cl.PhoneNumber,
                    CallDirection = cl.CallDirection,
                    CallStatus = cl.CallStatus,
                    CallOutcome = cl.CallOutcome,
                    Disposition = cl.Disposition,
                    AgentId = cl.AgentId,
                    StartedAt = cl.StartedAt,
                    EndedAt = cl.EndedAt,
                    DurationSeconds = cl.DurationSeconds,
                    TalkDurationSeconds = cl.TalkDurationSeconds
                }).ToList() ?? new List<CallLogSummaryDto>()
            };

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(leadDto);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lead {LeadId}", id);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }

    [Function("CreateLead")]
    public async Task<HttpResponseData> CreateLead([HttpTrigger(AuthorizationLevel.Function, "post", Route = "leads")] HttpRequestData req)
    {
        try
        {
            var body = await req.ReadAsStringAsync();
            if (string.IsNullOrEmpty(body))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Request body is required");
                return badRequest;
            }

            var createDto = JsonSerializer.Deserialize<CreateLeadDto>(body, _jsonOptions);
            if (createDto == null)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Invalid request body");
                return badRequest;
            }

            // Validate that the list exists
            var listExists = await _dbContext.Lists.AnyAsync(l => l.Id == createDto.ListId);
            if (!listExists)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync("List not found");
                return notFound;
            }

            // Validate required fields
            if (string.IsNullOrEmpty(createDto.PrimaryPhone))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Primary phone is required");
                return badRequest;
            }

            var lead = new Lead
            {
                ListId = createDto.ListId,
                FirstName = createDto.FirstName,
                LastName = createDto.LastName,
                FullName = createDto.FullName ?? $"{createDto.FirstName} {createDto.LastName}".Trim(),
                Company = createDto.Company,
                Title = createDto.Title,
                MiddleInitial = createDto.MiddleInitial,
                PrimaryPhone = createDto.PrimaryPhone,
                PhoneCode = createDto.PhoneCode,
                PhoneNumberRaw = createDto.PhoneNumberRaw,
                PhoneValidationStatus = createDto.PhoneValidationStatus,
                PhoneCarrier = createDto.PhoneCarrier,
                PhoneType = createDto.PhoneType,
                SecondaryPhone = createDto.SecondaryPhone,
                MobilePhone = createDto.MobilePhone,
                WorkPhone = createDto.WorkPhone,
                HomePhone = createDto.HomePhone,
                AlternatePhones = createDto.AlternatePhones,
                VendorLeadCode = createDto.VendorLeadCode,
                SourceId = createDto.SourceId,
                PrimaryEmail = createDto.PrimaryEmail,
                SecondaryEmail = createDto.SecondaryEmail,
                AddressLine1 = createDto.AddressLine1,
                AddressLine2 = createDto.AddressLine2,
                AddressLine3 = createDto.AddressLine3,
                City = createDto.City,
                State = createDto.State,
                Province = createDto.Province,
                PostalCode = createDto.PostalCode,
                Country = createDto.Country,
                TimeZone = createDto.TimeZone,
                Priority = createDto.Priority,
                Rank = createDto.Rank,
                QualityScore = createDto.QualityScore,
                LifecycleStage = createDto.LifecycleStage,
                Owner = createDto.Owner,
                EntryListId = createDto.EntryListId,
                GmtOffset = createDto.GmtOffset,
                GmtOffsetNow = createDto.GmtOffsetNow,
                SecurityPhrase = createDto.SecurityPhrase,
                ComplianceFlags = createDto.ComplianceFlags,
                CallbackAppointment = createDto.CallbackAppointment,
                Status = "NEW",
                CallAttempts = 0,
                CalledCount = 0,
                RecycleCount = 0,
                CalledSinceLastReset = false,
                Gender = createDto.Gender,
                DateOfBirth = createDto.DateOfBirth,
                MaritalStatus = createDto.MaritalStatus,
                Income = createDto.Income,
                Education = createDto.Education,
                Interests = createDto.Interests,
                Source = createDto.Source,
                SourceCampaign = createDto.SourceCampaign,
                LeadValue = createDto.LeadValue,
                ExpectedRevenue = createDto.ExpectedRevenue,
                Tags = createDto.Tags,
                CustomFields = createDto.CustomFields,
                ConversionTracking = createDto.ConversionTracking,
                InteractionHistory = createDto.InteractionHistory,
                ScoringFactors = createDto.ScoringFactors,
                AttributionData = createDto.AttributionData,
                Notes = createDto.Notes,
                IsExcluded = false,
                HasOptedOut = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ModifyDate = DateTime.UtcNow
            };

            _dbContext.Leads.Add(lead);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Created lead {LeadId} for list {ListId}", lead.Id, lead.ListId);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(new { id = lead.Id, message = "Lead created successfully" });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating lead");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }

    [Function("UpdateLead")]
    public async Task<HttpResponseData> UpdateLead([HttpTrigger(AuthorizationLevel.Function, "put", Route = "leads/{id:int}")] HttpRequestData req, int id)
    {
        try
        {
            var body = await req.ReadAsStringAsync();
            if (string.IsNullOrEmpty(body))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Request body is required");
                return badRequest;
            }

            var updateDto = JsonSerializer.Deserialize<UpdateLeadDto>(body, _jsonOptions);
            if (updateDto == null)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Invalid request body");
                return badRequest;
            }

            var lead = await _dbContext.Leads.FindAsync(id);
            if (lead == null)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync("Lead not found");
                return notFound;
            }

            // Update properties
            lead.FirstName = updateDto.FirstName;
            lead.LastName = updateDto.LastName;
            lead.FullName = updateDto.FullName ?? $"{updateDto.FirstName} {updateDto.LastName}".Trim();
            lead.Company = updateDto.Company;
            lead.Title = updateDto.Title;
            lead.MiddleInitial = updateDto.MiddleInitial;
            lead.PrimaryPhone = updateDto.PrimaryPhone;
            lead.PhoneCode = updateDto.PhoneCode;
            lead.PhoneNumberRaw = updateDto.PhoneNumberRaw;
            lead.PhoneValidationStatus = updateDto.PhoneValidationStatus;
            lead.PhoneCarrier = updateDto.PhoneCarrier;
            lead.PhoneType = updateDto.PhoneType;
            lead.SecondaryPhone = updateDto.SecondaryPhone;
            lead.MobilePhone = updateDto.MobilePhone;
            lead.WorkPhone = updateDto.WorkPhone;
            lead.HomePhone = updateDto.HomePhone;
            lead.AlternatePhones = updateDto.AlternatePhones;
            lead.VendorLeadCode = updateDto.VendorLeadCode;
            lead.SourceId = updateDto.SourceId;
            lead.PrimaryEmail = updateDto.PrimaryEmail;
            lead.SecondaryEmail = updateDto.SecondaryEmail;
            lead.AddressLine1 = updateDto.AddressLine1;
            lead.AddressLine2 = updateDto.AddressLine2;
            lead.AddressLine3 = updateDto.AddressLine3;
            lead.City = updateDto.City;
            lead.State = updateDto.State;
            lead.Province = updateDto.Province;
            lead.PostalCode = updateDto.PostalCode;
            lead.Country = updateDto.Country;
            lead.TimeZone = updateDto.TimeZone;
            lead.Priority = updateDto.Priority;
            lead.Rank = updateDto.Rank;
            lead.QualityScore = updateDto.QualityScore;
            lead.LifecycleStage = updateDto.LifecycleStage;
            lead.Owner = updateDto.Owner;
            lead.EntryListId = updateDto.EntryListId;
            lead.GmtOffset = updateDto.GmtOffset;
            lead.GmtOffsetNow = updateDto.GmtOffsetNow;
            lead.SecurityPhrase = updateDto.SecurityPhrase;
            lead.ComplianceFlags = updateDto.ComplianceFlags;
            lead.CallbackAppointment = updateDto.CallbackAppointment;
            lead.Status = updateDto.Status;
            lead.User = updateDto.User;
            lead.CallAttempts = updateDto.CallAttempts;
            lead.CalledCount = updateDto.CalledCount;
            lead.RecycleCount = updateDto.RecycleCount;
            lead.LastLocalCallTime = updateDto.LastLocalCallTime;
            lead.CalledSinceLastReset = updateDto.CalledSinceLastReset;
            lead.LastCalledAt = updateDto.LastCalledAt;
            lead.LastContactedAt = updateDto.LastContactedAt;
            lead.NextCallAt = updateDto.NextCallAt;
            lead.ScheduledCallbackAt = updateDto.ScheduledCallbackAt;
            lead.LastRecycledAt = updateDto.LastRecycledAt;
            lead.LastHandlerAgent = updateDto.LastHandlerAgent;
            lead.LastCallOutcome = updateDto.LastCallOutcome;
            lead.Disposition = updateDto.Disposition;
            lead.Gender = updateDto.Gender;
            lead.DateOfBirth = updateDto.DateOfBirth;
            lead.MaritalStatus = updateDto.MaritalStatus;
            lead.Income = updateDto.Income;
            lead.Education = updateDto.Education;
            lead.Interests = updateDto.Interests;
            lead.Source = updateDto.Source;
            lead.SourceCampaign = updateDto.SourceCampaign;
            lead.LeadValue = updateDto.LeadValue;
            lead.ExpectedRevenue = updateDto.ExpectedRevenue;
            lead.Tags = updateDto.Tags;
            lead.CustomFields = updateDto.CustomFields;
            lead.ConversionTracking = updateDto.ConversionTracking;
            lead.InteractionHistory = updateDto.InteractionHistory;
            lead.ScoringFactors = updateDto.ScoringFactors;
            lead.AttributionData = updateDto.AttributionData;
            lead.Notes = updateDto.Notes;
            lead.IsExcluded = updateDto.IsExcluded;
            lead.ExclusionReason = updateDto.ExclusionReason;
            lead.ExcludedAt = updateDto.ExcludedAt;
            lead.HasOptedOut = updateDto.HasOptedOut;
            lead.OptedOutAt = updateDto.OptedOutAt;
            lead.UpdatedAt = DateTime.UtcNow;
            lead.ModifyDate = updateDto.ModifyDate;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Updated lead {LeadId}", id);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new { message = "Lead updated successfully" });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lead {LeadId}", id);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }

    [Function("DeleteLead")]
    public async Task<HttpResponseData> DeleteLead([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "leads/{id:int}")] HttpRequestData req, int id)
    {
        try
        {
            var lead = await _dbContext.Leads.FindAsync(id);
            if (lead == null)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync("Lead not found");
                return notFound;
            }

            _dbContext.Leads.Remove(lead);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Deleted lead {LeadId}", id);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new { message = "Lead deleted successfully" });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting lead {LeadId}", id);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }

    [Function("BulkImportLeads")]
    public async Task<HttpResponseData> BulkImportLeads([HttpTrigger(AuthorizationLevel.Function, "post", Route = "lists/{listId:int}/leads/import")] HttpRequestData req, int listId)
    {
        try
        {
            var body = await req.ReadAsStringAsync();
            if (string.IsNullOrEmpty(body))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Request body is required");
                return badRequest;
            }

            var importDtos = JsonSerializer.Deserialize<List<CreateLeadDto>>(body, _jsonOptions);
            if (importDtos == null || !importDtos.Any())
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("No leads provided for import");
                return badRequest;
            }

            // Validate that the list exists
            var listExists = await _dbContext.Lists.AnyAsync(l => l.Id == listId);
            if (!listExists)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync("List not found");
                return notFound;
            }

            var validLeads = new List<Lead>();
            var errors = new List<string>();

            foreach (var (dto, index) in importDtos.Select((dto, i) => (dto, i)))
            {
                try
                {
                    if (string.IsNullOrEmpty(dto.PrimaryPhone))
                    {
                        errors.Add($"Row {index + 1}: Primary phone is required");
                        continue;
                    }

                    var lead = new Lead
                    {
                        ListId = listId,
                        FirstName = dto.FirstName,
                        LastName = dto.LastName,
                        FullName = dto.FullName ?? $"{dto.FirstName} {dto.LastName}".Trim(),
                        Company = dto.Company,
                        Title = dto.Title,
                        MiddleInitial = dto.MiddleInitial,
                        PrimaryPhone = dto.PrimaryPhone,
                        PhoneCode = dto.PhoneCode,
                        PhoneNumberRaw = dto.PhoneNumberRaw,
                        PhoneValidationStatus = dto.PhoneValidationStatus,
                        PhoneCarrier = dto.PhoneCarrier,
                        PhoneType = dto.PhoneType,
                        SecondaryPhone = dto.SecondaryPhone,
                        MobilePhone = dto.MobilePhone,
                        WorkPhone = dto.WorkPhone,
                        HomePhone = dto.HomePhone,
                        AlternatePhones = dto.AlternatePhones,
                        VendorLeadCode = dto.VendorLeadCode,
                        SourceId = dto.SourceId,
                        PrimaryEmail = dto.PrimaryEmail,
                        SecondaryEmail = dto.SecondaryEmail,
                        AddressLine1 = dto.AddressLine1,
                        AddressLine2 = dto.AddressLine2,
                        AddressLine3 = dto.AddressLine3,
                        City = dto.City,
                        State = dto.State,
                        Province = dto.Province,
                        PostalCode = dto.PostalCode,
                        Country = dto.Country,
                        TimeZone = dto.TimeZone,
                        Priority = dto.Priority,
                        Rank = dto.Rank,
                        QualityScore = dto.QualityScore,
                        LifecycleStage = dto.LifecycleStage,
                        Owner = dto.Owner,
                        EntryListId = dto.EntryListId,
                        GmtOffset = dto.GmtOffset,
                        GmtOffsetNow = dto.GmtOffsetNow,
                        SecurityPhrase = dto.SecurityPhrase,
                        ComplianceFlags = dto.ComplianceFlags,
                        CallbackAppointment = dto.CallbackAppointment,
                        Status = "NEW",
                        CallAttempts = 0,
                        CalledCount = 0,
                        RecycleCount = 0,
                        CalledSinceLastReset = false,
                        Gender = dto.Gender,
                        DateOfBirth = dto.DateOfBirth,
                        MaritalStatus = dto.MaritalStatus,
                        Income = dto.Income,
                        Education = dto.Education,
                        Interests = dto.Interests,
                        Source = dto.Source,
                        SourceCampaign = dto.SourceCampaign,
                        LeadValue = dto.LeadValue,
                        ExpectedRevenue = dto.ExpectedRevenue,
                        Tags = dto.Tags,
                        CustomFields = dto.CustomFields,
                        ConversionTracking = dto.ConversionTracking,
                        InteractionHistory = dto.InteractionHistory,
                        ScoringFactors = dto.ScoringFactors,
                        AttributionData = dto.AttributionData,
                        Notes = dto.Notes,
                        IsExcluded = false,
                        HasOptedOut = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        ModifyDate = DateTime.UtcNow
                    };

                    validLeads.Add(lead);
                }
                catch (Exception ex)
                {
                    errors.Add($"Row {index + 1}: {ex.Message}");
                }
            }

            if (validLeads.Any())
            {
                _dbContext.Leads.AddRange(validLeads);
                await _dbContext.SaveChangesAsync();
            }

            _logger.LogInformation("Bulk imported {Count} leads to list {ListId}", validLeads.Count, listId);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                message = "Bulk import completed",
                imported = validLeads.Count,
                errors = errors.Count,
                errorDetails = errors
            });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk import for list {ListId}", listId);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }
}
