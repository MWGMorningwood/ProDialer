using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProDialer.Functions.Data;
using ProDialer.Functions.Services;
using ProDialer.Shared.DTOs;
using ProDialer.Shared.Models;
using System.Net;
using System.Text.Json;

namespace ProDialer.Functions.Functions;

/// <summary>
/// Azure Functions for managing agents
/// Provides CRUD operations and agent status management
/// </summary>
public class AgentFunctions
{
    private readonly ProDialerDbContext _dbContext;
    private readonly TableStorageService _tableStorageService;
    private readonly ILogger<AgentFunctions> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public AgentFunctions(
        ProDialerDbContext dbContext, 
        TableStorageService tableStorageService, 
        ILogger<AgentFunctions> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _tableStorageService = tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    [Function("GetAgents")]
    public async Task<HttpResponseData> GetAgents([HttpTrigger(AuthorizationLevel.Function, "get", Route = "agents")] HttpRequestData req)
    {
        try
        {
            _logger.LogInformation("Retrieving agents");

            // Parse query parameters
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            var page = int.TryParse(query["page"], out var p) ? p : 1;
            var pageSize = int.TryParse(query["pageSize"], out var ps) ? Math.Min(ps, 100) : 20;
            var department = query["department"];
            var team = query["team"];
            var status = query["status"];
            var includeInactive = bool.TryParse(query["includeInactive"], out var inactive) && inactive;

            var agentsQuery = _dbContext.Agents.AsQueryable();

            // Apply filters
            if (!includeInactive)
            {
                agentsQuery = agentsQuery.Where(a => a.IsActive);
            }

            if (!string.IsNullOrEmpty(department))
            {
                agentsQuery = agentsQuery.Where(a => a.Role == department);
            }

            if (!string.IsNullOrEmpty(team))
            {
                agentsQuery = agentsQuery.Where(a => a.QualifiedCampaigns == team);
            }

            if (!string.IsNullOrEmpty(status))
            {
                agentsQuery = agentsQuery.Where(a => a.Status == status);
            }

            var totalCount = await agentsQuery.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var agents = await agentsQuery
                .OrderBy(a => a.LastName)
                .ThenBy(a => a.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AgentSummaryDto
                {
                    Id = a.Id,
                    FirstName = a.FirstName,
                    LastName = a.LastName,
                    FullName = a.FullName,
                    Email = a.Email,
                    Phone = a.PhoneNumber,
                    Department = a.Role,
                    Team = a.QualifiedCampaigns,
                    IsActive = a.IsActive,
                    Status = a.Status,
                    LastLoginAt = a.CurrentSessionStartedAt,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt,
                    // Performance metrics will be calculated separately for performance
                    TotalCalls = 0,
                    ConnectedCalls = 0,
                    AverageCallDuration = 0,
                    ConversionRate = 0
                })
                .ToListAsync();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                agents,
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
            _logger.LogError(ex, "Error retrieving agents");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }

    [Function("GetAgent")]
    public async Task<HttpResponseData> GetAgent([HttpTrigger(AuthorizationLevel.Function, "get", Route = "agents/{id:int}")] HttpRequestData req, int id)
    {
        try
        {
            _logger.LogInformation("Retrieving agent {AgentId}", id);

            var agent = await _dbContext.Agents.FindAsync(id);
            if (agent == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync("Agent not found");
                return notFoundResponse;
            }

            // Get performance metrics (simplified for now)
            var callLogs = await _dbContext.CallLogs
                .Where(cl => cl.AgentId == id.ToString())
                .ToListAsync();

            var todayCallLogs = callLogs.Where(cl => cl.CreatedAt.Date == DateTime.UtcNow.Date).ToList();

            var agentDto = new AgentDetailDto
            {
                Id = agent.Id,
                FirstName = agent.FirstName,
                LastName = agent.LastName,
                FullName = agent.FullName,
                Email = agent.Email,
                Phone = agent.PhoneNumber,
                Department = agent.Role,
                Team = agent.QualifiedCampaigns,
                Supervisor = agent.Supervisor,
                HireDate = null, // Not available in this model
                Skills = agent.Languages,
                Languages = agent.Languages,
                CanHandleInbound = true, // Not available in model, defaulting
                CanHandleOutbound = true, // Not available in model, defaulting
                HourlyRate = null, // Not available in this model
                Notes = null, // Not available in this model
                IsActive = agent.IsActive,
                Status = agent.Status,
                LastLoginAt = agent.CurrentSessionStartedAt,
                LastLogoutAt = agent.LastLoggedOutAt,
                CreatedAt = agent.CreatedAt,
                UpdatedAt = agent.UpdatedAt,
                
                // Performance metrics
                TotalCalls = callLogs.Count,
                ConnectedCalls = callLogs.Count(cl => cl.CallStatus == "Connected"),
                AverageCallDuration = callLogs.Count > 0 ? (decimal)callLogs.Average(cl => cl.DurationSeconds) : 0,
                TotalCallsToday = todayCallLogs.Count,
                ConnectedCallsToday = todayCallLogs.Count(cl => cl.CallStatus == "Connected"),
                TotalTalkTimeToday = todayCallLogs.Sum(cl => cl.DurationSeconds),
                AverageCallDurationToday = todayCallLogs.Count > 0 ? (decimal)todayCallLogs.Average(cl => cl.DurationSeconds) : 0,
                LeadsContactedToday = todayCallLogs.Count(cl => cl.CallStatus == "Connected"),
                AppointmentsSetToday = todayCallLogs.Count(cl => cl.Disposition == "Appointment Set")
            };

            agentDto.ConversionRate = agentDto.TotalCalls > 0 ? (decimal)agentDto.ConnectedCalls / agentDto.TotalCalls * 100 : 0;

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(agentDto);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving agent {AgentId}", id);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }

    [Function("CreateAgent")]
    public async Task<HttpResponseData> CreateAgent([HttpTrigger(AuthorizationLevel.Function, "post", Route = "agents")] HttpRequestData req)
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

            var createDto = JsonSerializer.Deserialize<CreateAgentDto>(body, _jsonOptions);
            if (createDto == null)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Invalid request body");
                return badRequest;
            }

            // Validate required fields
            if (string.IsNullOrEmpty(createDto.FirstName) || string.IsNullOrEmpty(createDto.LastName))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("First name and last name are required");
                return badRequest;
            }

            if (string.IsNullOrEmpty(createDto.Email))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Email is required");
                return badRequest;
            }

            // Check for duplicate email
            var existingAgent = await _dbContext.Agents.FirstOrDefaultAsync(a => a.Email == createDto.Email);
            if (existingAgent != null)
            {
                var conflict = req.CreateResponse(HttpStatusCode.Conflict);
                await conflict.WriteStringAsync("Agent with this email already exists");
                return conflict;
            }

            var agent = new Agent
            {
                UserId = createDto.Email, // Using email as user ID for now
                FirstName = createDto.FirstName,
                LastName = createDto.LastName,
                Email = createDto.Email,
                PhoneNumber = createDto.Phone,
                Role = createDto.Department,
                QualifiedCampaigns = createDto.Team,
                Supervisor = createDto.Supervisor,
                Languages = createDto.Languages,
                IsActive = true,
                Status = "Available",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System", // TODO: Get from authenticated user
                UpdatedBy = "System"
            };

            _dbContext.Agents.Add(agent);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Created agent {AgentId} - {FullName}", agent.Id, agent.FullName);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(new { id = agent.Id, message = "Agent created successfully" });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating agent");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }

    [Function("UpdateAgent")]
    public async Task<HttpResponseData> UpdateAgent([HttpTrigger(AuthorizationLevel.Function, "put", Route = "agents/{id:int}")] HttpRequestData req, int id)
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

            var updateDto = JsonSerializer.Deserialize<UpdateAgentDto>(body, _jsonOptions);
            if (updateDto == null)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Invalid request body");
                return badRequest;
            }

            var agent = await _dbContext.Agents.FindAsync(id);
            if (agent == null)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync("Agent not found");
                return notFound;
            }

            // Check for duplicate email (excluding current agent)
            if (updateDto.Email != agent.Email)
            {
                var existingAgent = await _dbContext.Agents.FirstOrDefaultAsync(a => a.Email == updateDto.Email && a.Id != id);
                if (existingAgent != null)
                {
                    var conflict = req.CreateResponse(HttpStatusCode.Conflict);
                    await conflict.WriteStringAsync("Another agent with this email already exists");
                    return conflict;
                }
            }

            // Update properties
            agent.FirstName = updateDto.FirstName;
            agent.LastName = updateDto.LastName;
            agent.Email = updateDto.Email;
            agent.PhoneNumber = updateDto.Phone;
            agent.Role = updateDto.Department;
            agent.QualifiedCampaigns = updateDto.Team;
            agent.Supervisor = updateDto.Supervisor;
            agent.Languages = updateDto.Languages;
            agent.IsActive = updateDto.IsActive;
            agent.Status = updateDto.Status;
            agent.UpdatedAt = DateTime.UtcNow;
            agent.UpdatedBy = "System"; // TODO: Get from authenticated user

            // Update login/logout tracking
            if (updateDto.Status == "Available" && agent.CurrentSessionStartedAt == null)
            {
                agent.CurrentSessionStartedAt = DateTime.UtcNow;
            }
            else if (updateDto.Status == "Offline")
            {
                agent.LastLoggedOutAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Updated agent {AgentId}", id);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new { message = "Agent updated successfully" });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating agent {AgentId}", id);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }

    [Function("DeleteAgent")]
    public async Task<HttpResponseData> DeleteAgent([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "agents/{id:int}")] HttpRequestData req, int id)
    {
        try
        {
            var agent = await _dbContext.Agents.FindAsync(id);
            if (agent == null)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync("Agent not found");
                return notFound;
            }

            // Soft delete by setting IsActive to false
            agent.IsActive = false;
            agent.Status = "Inactive";
            agent.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Soft deleted agent {AgentId}", id);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new { message = "Agent deactivated successfully" });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting agent {AgentId}", id);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }

    [Function("UpdateAgentStatus")]
    public async Task<HttpResponseData> UpdateAgentStatus([HttpTrigger(AuthorizationLevel.Function, "put", Route = "agents/{id:int}/status")] HttpRequestData req, int id)
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

            var statusDto = JsonSerializer.Deserialize<UpdateAgentStatusDto>(body, _jsonOptions);
            if (statusDto == null)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Invalid request body");
                return badRequest;
            }

            var agent = await _dbContext.Agents.FindAsync(id);
            if (agent == null)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync("Agent not found");
                return notFound;
            }

            // Update status in database
            agent.Status = statusDto.Status;
            agent.UpdatedAt = DateTime.UtcNow;

            // Update login/logout times based on status
            if (statusDto.Status == "Available" && agent.CurrentSessionStartedAt == null)
            {
                agent.CurrentSessionStartedAt = DateTime.UtcNow;
            }
            else if (statusDto.Status == "Offline")
            {
                agent.LastLoggedOutAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();

            // Also update real-time status in Table Storage
            try
            {
                await _tableStorageService.UpsertAgentStatusAsync(new AgentStatusDto
                {
                    AgentId = id,
                    Status = statusDto.Status,
                    StatusReason = statusDto.StatusReason,
                    StatusUpdatedAt = DateTime.UtcNow,
                    IsOnCall = false // This would be updated separately when handling calls
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update agent status in Table Storage for agent {AgentId}", id);
                // Continue execution - this is not critical
            }

            _logger.LogInformation("Updated status for agent {AgentId} to {Status}", id, statusDto.Status);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new { message = "Agent status updated successfully" });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating agent status for agent {AgentId}", id);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }

    [Function("GetAgentStatus")]
    public async Task<HttpResponseData> GetAgentStatus([HttpTrigger(AuthorizationLevel.Function, "get", Route = "agents/{id:int}/status")] HttpRequestData req, int id)
    {
        try
        {
            _logger.LogInformation("Retrieving status for agent {AgentId}", id);

            // First check if agent exists
            var agent = await _dbContext.Agents.FindAsync(id);
            if (agent == null)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync("Agent not found");
                return notFound;
            }

            // Try to get real-time status from Table Storage
            AgentStatusDto? statusDto = null;
            try
            {
                statusDto = await _tableStorageService.GetAgentStatusAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get agent status from Table Storage for agent {AgentId}", id);
            }

            // Fallback to database status if Table Storage is unavailable
            if (statusDto == null)
            {
                statusDto = new AgentStatusDto
                {
                    AgentId = id,
                    Status = agent.Status,
                    StatusUpdatedAt = agent.UpdatedAt,
                    IsOnCall = false
                };
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(statusDto);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving agent status for agent {AgentId}", id);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }

    [Function("GetAllAgentStatuses")]
    public async Task<HttpResponseData> GetAllAgentStatuses([HttpTrigger(AuthorizationLevel.Function, "get", Route = "agents/status")] HttpRequestData req)
    {
        try
        {
            _logger.LogInformation("Retrieving all agent statuses");

            var activeAgents = await _dbContext.Agents
                .Where(a => a.IsActive)
                .Select(a => new { a.Id, a.Status, a.UpdatedAt })
                .ToListAsync();

            var statuses = new List<AgentStatusDto>();

            foreach (var agent in activeAgents)
            {
                try
                {
                    var statusDto = await _tableStorageService.GetAgentStatusAsync(agent.Id);
                    if (statusDto != null)
                    {
                        statuses.Add(statusDto);
                    }
                    else
                    {
                        // Fallback to database status
                        statuses.Add(new AgentStatusDto
                        {
                            AgentId = agent.Id,
                            Status = agent.Status,
                            StatusUpdatedAt = agent.UpdatedAt,
                            IsOnCall = false
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get status for agent {AgentId}", agent.Id);
                    // Add fallback status
                    statuses.Add(new AgentStatusDto
                    {
                        AgentId = agent.Id,
                        Status = agent.Status,
                        StatusUpdatedAt = agent.UpdatedAt,
                        IsOnCall = false
                    });
                }
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(statuses);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all agent statuses");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }
}
