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
                    l.FirstName.Contains(search) ||
                    l.LastName.Contains(search) ||
                    l.FullName.Contains(search) ||
                    l.Company.Contains(search) ||
                    l.PrimaryPhone.Contains(search) ||
                    l.PrimaryEmail.Contains(search));
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
                    Priority = l.Priority,
                    CallCount = l.CallAttempts,
                    LastCallAttempt = l.LastCalledAt,
                    ScheduledCallbackAt = l.ScheduledCallbackAt,
                    Disposition = l.Disposition,
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
                PrimaryPhone = lead.PrimaryPhone,
                SecondaryPhone = lead.SecondaryPhone,
                MobilePhone = lead.MobilePhone,
                WorkPhone = lead.WorkPhone,
                HomePhone = lead.HomePhone,
                PrimaryEmail = lead.PrimaryEmail,
                SecondaryEmail = lead.SecondaryEmail,
                AddressLine1 = lead.AddressLine1,
                AddressLine2 = lead.AddressLine2,
                City = lead.City,
                State = lead.State,
                PostalCode = lead.PostalCode,
                Country = lead.Country,
                TimeZone = lead.TimeZone,
                Priority = lead.Priority,
                Status = lead.Status,
                CallCount = lead.CallAttempts,
                LastCallAttempt = lead.LastCalledAt,
                ScheduledCallbackAt = lead.ScheduledCallbackAt,
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
                Notes = lead.Notes,
                IsExcluded = lead.IsExcluded,
                ExclusionReason = lead.ExclusionReason,
                HasOptedOut = lead.HasOptedOut,
                CreatedAt = lead.CreatedAt,
                UpdatedAt = lead.UpdatedAt,
                CallHistory = lead.CallLogs?.Select(cl => new CallLogSummaryDto
                {
                    Id = cl.Id,
                    CallDirection = cl.CallDirection,
                    CallStatus = cl.CallStatus,
                    CallDuration = cl.DurationSeconds,
                    Disposition = cl.Disposition,
                    Notes = cl.Notes,
                    CreatedAt = cl.StartedAt
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
                PrimaryPhone = createDto.PrimaryPhone,
                SecondaryPhone = createDto.SecondaryPhone,
                MobilePhone = createDto.MobilePhone,
                WorkPhone = createDto.WorkPhone,
                HomePhone = createDto.HomePhone,
                PrimaryEmail = createDto.PrimaryEmail,
                SecondaryEmail = createDto.SecondaryEmail,
                AddressLine1 = createDto.AddressLine1,
                AddressLine2 = createDto.AddressLine2,
                City = createDto.City,
                State = createDto.State,
                PostalCode = createDto.PostalCode,
                Country = createDto.Country,
                TimeZone = createDto.TimeZone,
                Priority = createDto.Priority,
                Status = "New",
                CallAttempts = 0,
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
                Notes = createDto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
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
            lead.PrimaryPhone = updateDto.PrimaryPhone;
            lead.SecondaryPhone = updateDto.SecondaryPhone;
            lead.MobilePhone = updateDto.MobilePhone;
            lead.WorkPhone = updateDto.WorkPhone;
            lead.HomePhone = updateDto.HomePhone;
            lead.PrimaryEmail = updateDto.PrimaryEmail;
            lead.SecondaryEmail = updateDto.SecondaryEmail;
            lead.AddressLine1 = updateDto.AddressLine1;
            lead.AddressLine2 = updateDto.AddressLine2;
            lead.City = updateDto.City;
            lead.State = updateDto.State;
            lead.PostalCode = updateDto.PostalCode;
            lead.Country = updateDto.Country;
            lead.TimeZone = updateDto.TimeZone;
            lead.Priority = updateDto.Priority;
            lead.Status = updateDto.Status;
            lead.ScheduledCallbackAt = updateDto.ScheduledCallbackAt;
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
            lead.Notes = updateDto.Notes;
            lead.IsExcluded = updateDto.IsExcluded;
            lead.ExclusionReason = updateDto.ExclusionReason;
            lead.HasOptedOut = updateDto.HasOptedOut;
            lead.UpdatedAt = DateTime.UtcNow;

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
                        PrimaryPhone = dto.PrimaryPhone,
                        SecondaryPhone = dto.SecondaryPhone,
                        MobilePhone = dto.MobilePhone,
                        WorkPhone = dto.WorkPhone,
                        HomePhone = dto.HomePhone,
                        PrimaryEmail = dto.PrimaryEmail,
                        SecondaryEmail = dto.SecondaryEmail,
                        AddressLine1 = dto.AddressLine1,
                        AddressLine2 = dto.AddressLine2,
                        City = dto.City,
                        State = dto.State,
                        PostalCode = dto.PostalCode,
                        Country = dto.Country,
                        TimeZone = dto.TimeZone,
                        Priority = dto.Priority,
                        Status = "New",
                        CallAttempts = 0,
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
                        Notes = dto.Notes,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
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
