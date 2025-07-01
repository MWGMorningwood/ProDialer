using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ProDialer.Functions.Services;

/// <summary>
/// Service for managing non-relational data in Azure Table Storage
/// Handles agent sessions, real-time status, and other transient data
/// </summary>
public class TableStorageService
{
    private readonly TableServiceClient _tableServiceClient;
    private readonly ILogger<TableStorageService> _logger;
    
    // Table names for different entity types
    private const string AgentSessionsTable = "AgentSessions";
    private const string CampaignStatusTable = "CampaignStatus";
    private const string CallQueueTable = "CallQueue";
    private const string PreviewSessionsTable = "PreviewSessions";
    private const string CallSessionsTable = "CallSessions";
    private const string ManualDialQueueTable = "ManualDialQueue";

    public TableStorageService(TableServiceClient tableServiceClient, ILogger<TableStorageService> logger)
    {
        _tableServiceClient = tableServiceClient ?? throw new ArgumentNullException(nameof(tableServiceClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Initializes all required tables if they don't exist
    /// </summary>
    public async Task InitializeTablesAsync()
    {
        try
        {
            var tableNames = new[] { AgentSessionsTable, CampaignStatusTable, CallQueueTable, PreviewSessionsTable, CallSessionsTable, ManualDialQueueTable };
            
            foreach (var tableName in tableNames)
            {
                var tableClient = _tableServiceClient.GetTableClient(tableName);
                await tableClient.CreateIfNotExistsAsync();
                _logger.LogInformation("Table {TableName} initialized", tableName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize tables");
            throw;
        }
    }

    #region Agent Sessions

    /// <summary>
    /// Creates or updates an agent session record
    /// </summary>
    public async Task<bool> UpsertAgentSessionAsync(string agentId, object sessionData)
    {
        try
        {
            var tableClient = _tableServiceClient.GetTableClient(AgentSessionsTable);
            
            var entity = new TableEntity("AgentSession", agentId)
            {
                ["SessionData"] = JsonSerializer.Serialize(sessionData),
                ["LastUpdated"] = DateTime.UtcNow,
                ["Status"] = "Active"
            };

            await tableClient.UpsertEntityAsync(entity);
            _logger.LogInformation("Agent session updated for agent {AgentId}", agentId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upsert agent session for agent {AgentId}", agentId);
            return false;
        }
    }

    /// <summary>
    /// Gets an agent session by agent ID
    /// </summary>
    public async Task<T?> GetAgentSessionAsync<T>(string agentId) where T : class
    {
        try
        {
            var tableClient = _tableServiceClient.GetTableClient(AgentSessionsTable);
            var response = await tableClient.GetEntityIfExistsAsync<TableEntity>("AgentSession", agentId);
            
            if (!response.HasValue)
            {
                return null;
            }

            var entity = response.Value;
            if (entity.TryGetValue("SessionData", out var sessionDataObj) && sessionDataObj is string sessionData)
            {
                return JsonSerializer.Deserialize<T>(sessionData);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get agent session for agent {AgentId}", agentId);
            return null;
        }
    }

    /// <summary>
    /// Removes an agent session
    /// </summary>
    public async Task<bool> RemoveAgentSessionAsync(string agentId)
    {
        try
        {
            var tableClient = _tableServiceClient.GetTableClient(AgentSessionsTable);
            await tableClient.DeleteEntityAsync("AgentSession", agentId);
            _logger.LogInformation("Agent session removed for agent {AgentId}", agentId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove agent session for agent {AgentId}", agentId);
            return false;
        }
    }

    #endregion

    #region Campaign Status

    /// <summary>
    /// Updates campaign real-time status information
    /// </summary>
    public async Task<bool> UpdateCampaignStatusAsync(string campaignId, object statusData)
    {
        try
        {
            var tableClient = _tableServiceClient.GetTableClient(CampaignStatusTable);
            
            var entity = new TableEntity("CampaignStatus", campaignId)
            {
                ["StatusData"] = JsonSerializer.Serialize(statusData),
                ["LastUpdated"] = DateTime.UtcNow
            };

            await tableClient.UpsertEntityAsync(entity);
            _logger.LogInformation("Campaign status updated for campaign {CampaignId}", campaignId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update campaign status for campaign {CampaignId}", campaignId);
            return false;
        }
    }

    /// <summary>
    /// Gets campaign real-time status
    /// </summary>
    public async Task<T?> GetCampaignStatusAsync<T>(string campaignId) where T : class
    {
        try
        {
            var tableClient = _tableServiceClient.GetTableClient(CampaignStatusTable);
            var response = await tableClient.GetEntityIfExistsAsync<TableEntity>("CampaignStatus", campaignId);
            
            if (!response.HasValue)
            {
                return null;
            }

            var entity = response.Value;
            if (entity.TryGetValue("StatusData", out var statusDataObj) && statusDataObj is string statusData)
            {
                return JsonSerializer.Deserialize<T>(statusData);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get campaign status for campaign {CampaignId}", campaignId);
            return null;
        }
    }

    #endregion

    #region Call Queue

    /// <summary>
    /// Adds a call to the processing queue
    /// </summary>
    public async Task<bool> EnqueueCallAsync(string callId, object callData)
    {
        try
        {
            var tableClient = _tableServiceClient.GetTableClient(CallQueueTable);
            
            var entity = new TableEntity("CallQueue", callId)
            {
                ["CallData"] = JsonSerializer.Serialize(callData),
                ["QueuedAt"] = DateTime.UtcNow,
                ["Status"] = "Queued",
                ["Priority"] = 1
            };

            await tableClient.AddEntityAsync(entity);
            _logger.LogInformation("Call queued with ID {CallId}", callId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue call with ID {CallId}", callId);
            return false;
        }
    }

    /// <summary>
    /// Gets the next call from the queue based on priority
    /// </summary>
    public async Task<(string? CallId, T? CallData)> DequeueCallAsync<T>() where T : class
    {
        try
        {
            var tableClient = _tableServiceClient.GetTableClient(CallQueueTable);
            
            // Query for queued calls ordered by priority and queue time
            var query = tableClient.QueryAsync<TableEntity>(
                filter: "PartitionKey eq 'CallQueue' and Status eq 'Queued'",
                maxPerPage: 1);

            await foreach (var entity in query)
            {
                // Mark as processing
                entity["Status"] = "Processing";
                entity["ProcessingStarted"] = DateTime.UtcNow;
                
                await tableClient.UpdateEntityAsync(entity, entity.ETag);

                if (entity.TryGetValue("CallData", out var callDataObj) && callDataObj is string callData)
                {
                    var deserializedData = JsonSerializer.Deserialize<T>(callData);
                    return (entity.RowKey, deserializedData);
                }
            }

            return (null, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to dequeue call");
            return (null, null);
        }
    }

    /// <summary>
    /// Removes a call from the queue
    /// </summary>
    public async Task<bool> RemoveCallFromQueueAsync(string callId)
    {
        try
        {
            var tableClient = _tableServiceClient.GetTableClient(CallQueueTable);
            await tableClient.DeleteEntityAsync("CallQueue", callId);
            _logger.LogInformation("Call removed from queue with ID {CallId}", callId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove call from queue with ID {CallId}", callId);
            return false;
        }
    }

    #endregion

    #region Agent Status

    /// <summary>
    /// Updates or inserts an agent's status in Table Storage
    /// </summary>
    public async Task UpsertAgentStatusAsync(ProDialer.Shared.DTOs.AgentStatusDto statusDto)
    {
        try
        {
            var tableClient = _tableServiceClient.GetTableClient("AgentStatus");
            await tableClient.CreateIfNotExistsAsync();
            
            var entity = new TableEntity("AgentStatus", statusDto.AgentId.ToString())
            {
                ["Status"] = statusDto.Status,
                ["StatusReason"] = statusDto.StatusReason,
                ["StatusUpdatedAt"] = statusDto.StatusUpdatedAt,
                ["IsOnCall"] = statusDto.IsOnCall,
                ["CurrentCallId"] = statusDto.CurrentCallId,
                ["CurrentCallStatus"] = statusDto.CurrentCallStatus,
                ["CallStartedAt"] = statusDto.CallStartedAt
            };

            await tableClient.UpsertEntityAsync(entity);
            _logger.LogInformation("Updated agent status for agent {AgentId}", statusDto.AgentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update agent status for agent {AgentId}", statusDto.AgentId);
            throw;
        }
    }

    /// <summary>
    /// Gets an agent's current status from Table Storage
    /// </summary>
    public async Task<ProDialer.Shared.DTOs.AgentStatusDto?> GetAgentStatusAsync(int agentId)
    {
        try
        {
            var tableClient = _tableServiceClient.GetTableClient("AgentStatus");
            
            var response = await tableClient.GetEntityIfExistsAsync<TableEntity>("AgentStatus", agentId.ToString());
            if (!response.HasValue)
            {
                return null;
            }

            var entity = response.Value!;
            return new ProDialer.Shared.DTOs.AgentStatusDto
            {
                AgentId = agentId,
                Status = entity.GetString("Status") ?? "Unknown",
                StatusReason = entity.GetString("StatusReason"),
                StatusUpdatedAt = entity.GetDateTime("StatusUpdatedAt") ?? DateTime.UtcNow,
                IsOnCall = entity.GetBoolean("IsOnCall") ?? false,
                CurrentCallId = entity.GetInt32("CurrentCallId"),
                CurrentCallStatus = entity.GetString("CurrentCallStatus"),
                CallStartedAt = entity.GetDateTime("CallStartedAt")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get agent status for agent {AgentId}", agentId);
            return null;
        }
    }

    /// <summary>
    /// Gets all agent statuses from Table Storage
    /// </summary>
    public async Task<List<ProDialer.Shared.DTOs.AgentStatusDto>> GetAllAgentStatusesAsync()
    {
        try
        {
            var tableClient = _tableServiceClient.GetTableClient("AgentStatus");
            var statuses = new List<ProDialer.Shared.DTOs.AgentStatusDto>();

            await foreach (var entity in tableClient.QueryAsync<TableEntity>(filter: "PartitionKey eq 'AgentStatus'"))
            {
                if (int.TryParse(entity.RowKey, out var agentId))
                {
                    statuses.Add(new ProDialer.Shared.DTOs.AgentStatusDto
                    {
                        AgentId = agentId,
                        Status = entity.GetString("Status") ?? "Unknown",
                        StatusReason = entity.GetString("StatusReason"),
                        StatusUpdatedAt = entity.GetDateTime("StatusUpdatedAt") ?? DateTime.UtcNow,
                        IsOnCall = entity.GetBoolean("IsOnCall") ?? false,
                        CurrentCallId = entity.GetInt32("CurrentCallId"),
                        CurrentCallStatus = entity.GetString("CurrentCallStatus"),
                        CallStartedAt = entity.GetDateTime("CallStartedAt")
                    });
                }
            }

            return statuses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all agent statuses");
            return new List<ProDialer.Shared.DTOs.AgentStatusDto>();
        }
    }

    #endregion

    #region Preview Sessions

    /// <summary>
    /// Stores a preview session for agent review
    /// </summary>
    public async Task<bool> StorePreviewSessionAsync(int campaignId, int leadId, int agentId)
    {
        try
        {
            var tableClient = _tableServiceClient.GetTableClient(PreviewSessionsTable);
            
            var entity = new TableEntity
            {
                PartitionKey = $"Campaign_{campaignId}",
                RowKey = $"Lead_{leadId}_Agent_{agentId}_{DateTime.UtcNow.Ticks}",
                ["CampaignId"] = campaignId,
                ["LeadId"] = leadId,
                ["AgentId"] = agentId,
                ["CreatedAt"] = DateTime.UtcNow,
                ["Status"] = "PENDING"
            };

            await tableClient.UpsertEntityAsync(entity);
            
            _logger.LogDebug("Preview session stored for campaign {CampaignId}, lead {LeadId}, agent {AgentId}", 
                campaignId, leadId, agentId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store preview session");
            return false;
        }
    }

    #endregion

    #region Call Sessions

    /// <summary>
    /// Stores an active call session
    /// </summary>
    public async Task<bool> StoreCallSessionAsync(int campaignId, int leadId, string callConnectionId)
    {
        try
        {
            var tableClient = _tableServiceClient.GetTableClient(CallSessionsTable);
            
            var entity = new TableEntity
            {
                PartitionKey = $"Campaign_{campaignId}",
                RowKey = callConnectionId,
                ["CampaignId"] = campaignId,
                ["LeadId"] = leadId,
                ["CallConnectionId"] = callConnectionId,
                ["StartedAt"] = DateTime.UtcNow,
                ["Status"] = "ACTIVE"
            };

            await tableClient.UpsertEntityAsync(entity);
            
            _logger.LogDebug("Call session stored for campaign {CampaignId}, lead {LeadId}, connection {CallConnectionId}", 
                campaignId, leadId, callConnectionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store call session");
            return false;
        }
    }

    /// <summary>
    /// Updates call session status
    /// </summary>
    public async Task<bool> UpdateCallSessionAsync(string callConnectionId, string status, int? agentId = null)
    {
        try
        {
            var tableClient = _tableServiceClient.GetTableClient(CallSessionsTable);
            
            // Find the entity by call connection ID across all partitions
            await foreach (var entity in tableClient.QueryAsync<TableEntity>(filter: $"CallConnectionId eq '{callConnectionId}'"))
            {
                entity["Status"] = status;
                entity["UpdatedAt"] = DateTime.UtcNow;
                
                if (agentId.HasValue)
                {
                    entity["AgentId"] = agentId.Value;
                }

                await tableClient.UpdateEntityAsync(entity, entity.ETag);
                return true;
            }

            _logger.LogWarning("Call session not found for connection ID {CallConnectionId}", callConnectionId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update call session");
            return false;
        }
    }

    #endregion

    #region Manual Dial Queue

    /// <summary>
    /// Adds a lead to an agent's manual dial queue
    /// </summary>
    public async Task<bool> AddToManualQueueAsync(int agentId, int leadId)
    {
        try
        {
            var tableClient = _tableServiceClient.GetTableClient(ManualDialQueueTable);
            
            var entity = new TableEntity
            {
                PartitionKey = $"Agent_{agentId}",
                RowKey = $"Lead_{leadId}_{DateTime.UtcNow.Ticks}",
                ["AgentId"] = agentId,
                ["LeadId"] = leadId,
                ["QueuedAt"] = DateTime.UtcNow,
                ["Status"] = "QUEUED"
            };

            await tableClient.UpsertEntityAsync(entity);
            
            _logger.LogDebug("Lead {LeadId} added to manual dial queue for agent {AgentId}", leadId, agentId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add lead to manual dial queue");
            return false;
        }
    }

    /// <summary>
    /// Gets manual dial queue for an agent
    /// </summary>
    public async Task<List<int>> GetManualDialQueueAsync(int agentId)
    {
        try
        {
            var tableClient = _tableServiceClient.GetTableClient(ManualDialQueueTable);
            var leadIds = new List<int>();

            await foreach (var entity in tableClient.QueryAsync<TableEntity>(
                filter: $"PartitionKey eq 'Agent_{agentId}' and Status eq 'QUEUED'"))
            {
                if (entity.GetInt32("LeadId") is int leadId)
                {
                    leadIds.Add(leadId);
                }
            }

            return leadIds.OrderBy(x => x).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get manual dial queue for agent {AgentId}", agentId);
            return new List<int>();
        }
    }

    /// <summary>
    /// Removes a lead from manual dial queue
    /// </summary>
    public async Task<bool> RemoveFromManualQueueAsync(int agentId, int leadId)
    {
        try
        {
            var tableClient = _tableServiceClient.GetTableClient(ManualDialQueueTable);

            await foreach (var entity in tableClient.QueryAsync<TableEntity>(
                filter: $"PartitionKey eq 'Agent_{agentId}' and LeadId eq {leadId}"))
            {
                await tableClient.DeleteEntityAsync(entity.PartitionKey, entity.RowKey);
                _logger.LogDebug("Lead {LeadId} removed from manual dial queue for agent {AgentId}", leadId, agentId);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove lead from manual dial queue");
            return false;
        }
    }

    #endregion
}
