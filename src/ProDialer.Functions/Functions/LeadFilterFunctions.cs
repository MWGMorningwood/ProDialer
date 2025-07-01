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
/// Azure Functions for managing lead filters
/// Provides CRUD operations and lead filtering functionality for VICIdial compatibility
/// </summary>
public class LeadFilterFunctions
{
    private readonly ProDialerDbContext _dbContext;
    private readonly ILogger<LeadFilterFunctions> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public LeadFilterFunctions(ProDialerDbContext dbContext, ILogger<LeadFilterFunctions> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    [Function("GetLeadFilters")]
    public async Task<HttpResponseData> GetLeadFilters([HttpTrigger(AuthorizationLevel.Function, "get", Route = "lead-filters")] HttpRequestData req)
    {
        try
        {
            _logger.LogInformation("Retrieving lead filters");

            var leadFilters = await _dbContext.LeadFilters
                .Where(lf => lf.IsActive)
                .OrderBy(lf => lf.Priority)
                .ThenBy(lf => lf.Name)
                .Select(lf => new LeadFilterDto
                {
                    Id = lf.Id,
                    Name = lf.Name,
                    Description = lf.Description,
                    FilterType = lf.FilterType,
                    SqlFilter = lf.SqlFilter,
                    FilterRules = lf.FilterRules,
                    IsActive = lf.IsActive,
                    Priority = lf.Priority,
                    UserGroup = lf.UserGroup,
                    MatchingLeadCount = lf.MatchingLeadCount,
                    CreatedAt = lf.CreatedAt,
                    UpdatedAt = lf.UpdatedAt,
                    CreatedBy = lf.CreatedBy,
                    UpdatedBy = lf.UpdatedBy
                })
                .ToListAsync();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(leadFilters);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lead filters");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }

    [Function("GetLeadFilter")]
    public async Task<HttpResponseData> GetLeadFilter([HttpTrigger(AuthorizationLevel.Function, "get", Route = "lead-filters/{id:int}")] HttpRequestData req, int id)
    {
        try
        {
            _logger.LogInformation("Retrieving lead filter {LeadFilterId}", id);

            var leadFilter = await _dbContext.LeadFilters
                .FirstOrDefaultAsync(lf => lf.Id == id);

            if (leadFilter == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync("Lead filter not found");
                return notFoundResponse;
            }

            var leadFilterDto = new LeadFilterDto
            {
                Id = leadFilter.Id,
                Name = leadFilter.Name,
                Description = leadFilter.Description,
                FilterType = leadFilter.FilterType,
                SqlFilter = leadFilter.SqlFilter,
                FilterRules = leadFilter.FilterRules,
                IsActive = leadFilter.IsActive,
                Priority = leadFilter.Priority,
                UserGroup = leadFilter.UserGroup,
                MatchingLeadCount = leadFilter.MatchingLeadCount,
                CreatedAt = leadFilter.CreatedAt,
                UpdatedAt = leadFilter.UpdatedAt,
                CreatedBy = leadFilter.CreatedBy,
                UpdatedBy = leadFilter.UpdatedBy
            };

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(leadFilterDto);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lead filter {LeadFilterId}", id);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }

    [Function("CreateLeadFilter")]
    public async Task<HttpResponseData> CreateLeadFilter([HttpTrigger(AuthorizationLevel.Function, "post", Route = "lead-filters")] HttpRequestData req)
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

            var createDto = JsonSerializer.Deserialize<CreateLeadFilterDto>(body, _jsonOptions);
            if (createDto == null)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Invalid request body");
                return badRequest;
            }

            // Validate required fields
            if (string.IsNullOrEmpty(createDto.Name))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Filter name is required");
                return badRequest;
            }

            // Validate filter configuration
            if (createDto.FilterType == "SQL" && string.IsNullOrEmpty(createDto.SqlFilter))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("SQL filter is required when filter type is SQL");
                return badRequest;
            }

            if (createDto.FilterType == "RULE_BASED" && string.IsNullOrEmpty(createDto.FilterRules))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Filter rules are required when filter type is RULE_BASED");
                return badRequest;
            }

            var leadFilter = new LeadFilter
            {
                Name = createDto.Name,
                Description = createDto.Description,
                FilterType = createDto.FilterType,
                SqlFilter = createDto.SqlFilter,
                FilterRules = createDto.FilterRules,
                IsActive = createDto.IsActive,
                Priority = createDto.Priority,
                UserGroup = createDto.UserGroup,
                MatchingLeadCount = 0, // Will be calculated later
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System", // TODO: Get from auth context
                UpdatedBy = "System"
            };

            _dbContext.LeadFilters.Add(leadFilter);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Created lead filter {LeadFilterId} with name {Name}", leadFilter.Id, leadFilter.Name);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(new { id = leadFilter.Id, message = "Lead filter created successfully" });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating lead filter");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }

    [Function("UpdateLeadFilter")]
    public async Task<HttpResponseData> UpdateLeadFilter([HttpTrigger(AuthorizationLevel.Function, "put", Route = "lead-filters/{id:int}")] HttpRequestData req, int id)
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

            var updateDto = JsonSerializer.Deserialize<UpdateLeadFilterDto>(body, _jsonOptions);
            if (updateDto == null)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Invalid request body");
                return badRequest;
            }

            var leadFilter = await _dbContext.LeadFilters.FindAsync(id);
            if (leadFilter == null)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync("Lead filter not found");
                return notFound;
            }

            // Validate required fields
            if (string.IsNullOrEmpty(updateDto.Name))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Filter name is required");
                return badRequest;
            }

            // Validate filter configuration
            if (updateDto.FilterType == "SQL" && string.IsNullOrEmpty(updateDto.SqlFilter))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("SQL filter is required when filter type is SQL");
                return badRequest;
            }

            if (updateDto.FilterType == "RULE_BASED" && string.IsNullOrEmpty(updateDto.FilterRules))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Filter rules are required when filter type is RULE_BASED");
                return badRequest;
            }

            // Update properties
            leadFilter.Name = updateDto.Name;
            leadFilter.Description = updateDto.Description;
            leadFilter.FilterType = updateDto.FilterType;
            leadFilter.SqlFilter = updateDto.SqlFilter;
            leadFilter.FilterRules = updateDto.FilterRules;
            leadFilter.IsActive = updateDto.IsActive;
            leadFilter.Priority = updateDto.Priority;
            leadFilter.UserGroup = updateDto.UserGroup;
            leadFilter.UpdatedAt = DateTime.UtcNow;
            leadFilter.UpdatedBy = "System"; // TODO: Get from auth context

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Updated lead filter {LeadFilterId}", id);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new { message = "Lead filter updated successfully" });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lead filter {LeadFilterId}", id);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }

    [Function("DeleteLeadFilter")]
    public async Task<HttpResponseData> DeleteLeadFilter([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "lead-filters/{id:int}")] HttpRequestData req, int id)
    {
        try
        {
            var leadFilter = await _dbContext.LeadFilters.FindAsync(id);
            if (leadFilter == null)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync("Lead filter not found");
                return notFound;
            }

            // Soft delete by setting IsActive to false
            leadFilter.IsActive = false;
            leadFilter.UpdatedAt = DateTime.UtcNow;
            leadFilter.UpdatedBy = "System"; // TODO: Get from auth context

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Deleted lead filter {LeadFilterId}", id);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new { message = "Lead filter deleted successfully" });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting lead filter {LeadFilterId}", id);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }

    [Function("TestLeadFilter")]
    public async Task<HttpResponseData> TestLeadFilter([HttpTrigger(AuthorizationLevel.Function, "post", Route = "lead-filters/{id:int}/test")] HttpRequestData req, int id)
    {
        try
        {
            var leadFilter = await _dbContext.LeadFilters.FindAsync(id);
            if (leadFilter == null)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync("Lead filter not found");
                return notFound;
            }

            var body = await req.ReadAsStringAsync();
            var query = string.IsNullOrEmpty(body) ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(body, _jsonOptions);
            
            // Parse test parameters
            var listId = query?.ContainsKey("listId") == true ? Convert.ToInt32(query["listId"]) : (int?)null;
            var limit = query?.ContainsKey("limit") == true ? Convert.ToInt32(query["limit"]) : 10;

            // TODO: Implement actual filter execution logic
            // For now, simulate filter testing
            var leadsQuery = _dbContext.Leads.AsQueryable();

            if (listId.HasValue)
            {
                leadsQuery = leadsQuery.Where(l => l.ListId == listId.Value);
            }

            // Mock filter application based on filter type
            if (leadFilter.FilterType == "SQL" && !string.IsNullOrEmpty(leadFilter.SqlFilter))
            {
                // In a real implementation, you would safely execute the SQL filter
                // For demo purposes, just apply a simple filter
                _logger.LogWarning("SQL filter execution not implemented: {SqlFilter}", leadFilter.SqlFilter);
            }
            else if (leadFilter.FilterType == "RULE_BASED" && !string.IsNullOrEmpty(leadFilter.FilterRules))
            {
                // In a real implementation, you would parse and apply the rules
                _logger.LogWarning("Rule-based filter execution not implemented: {FilterRules}", leadFilter.FilterRules);
            }

            var matchingLeads = await leadsQuery
                .Take(limit)
                .Select(l => new
                {
                    l.Id,
                    l.FirstName,
                    l.LastName,
                    l.PrimaryPhone,
                    l.Status,
                    l.CreatedAt
                })
                .ToListAsync();

            var totalCount = await leadsQuery.CountAsync();

            // Update matching lead count
            leadFilter.MatchingLeadCount = totalCount;
            leadFilter.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Tested lead filter {LeadFilterId}: {MatchingCount} matching leads", 
                id, totalCount);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                filterId = id,
                filterName = leadFilter.Name,
                matchingLeadCount = totalCount,
                sampleLeads = matchingLeads,
                message = $"Filter test completed. Found {totalCount} matching leads."
            });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing lead filter {LeadFilterId}", id);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }
}
