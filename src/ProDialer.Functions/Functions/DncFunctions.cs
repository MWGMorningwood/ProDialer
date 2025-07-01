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
/// Azure Functions for managing Do Not Call (DNC) lists and numbers
/// Provides CRUD operations and DNC compliance functionality for VICIdial compatibility
/// </summary>
public class DncFunctions
{
    private readonly ProDialerDbContext _dbContext;
    private readonly ILogger<DncFunctions> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public DncFunctions(ProDialerDbContext dbContext, ILogger<DncFunctions> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    #region DNC Lists

    [Function("GetDncLists")]
    public async Task<HttpResponseData> GetDncLists([HttpTrigger(AuthorizationLevel.Function, "get", Route = "dnc-lists")] HttpRequestData req)
    {
        try
        {
            _logger.LogInformation("Retrieving DNC lists");

            var dncLists = await _dbContext.DncLists
                .Include(dl => dl.Campaign)
                .Include(dl => dl.List)
                .Where(dl => dl.IsActive)
                .OrderBy(dl => dl.Name)
                .Select(dl => new DncListDto
                {
                    Id = dl.Id,
                    Name = dl.Name,
                    Description = dl.Description,
                    ListType = dl.ListType,
                    IsActive = dl.IsActive,
                    Scope = dl.Scope,
                    TotalNumbers = dl.TotalNumbers,
                    Source = dl.Source,
                    AutoScrubbing = dl.AutoScrubbing,
                    CampaignId = dl.CampaignId,
                    CampaignName = dl.Campaign != null ? dl.Campaign.Name : null,
                    ListId = dl.ListId,
                    ListName = dl.List != null ? dl.List.Name : null,
                    CreatedAt = dl.CreatedAt,
                    UpdatedAt = dl.UpdatedAt,
                    CreatedBy = dl.CreatedBy,
                    UpdatedBy = dl.UpdatedBy
                })
                .ToListAsync();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(dncLists);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving DNC lists");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }

    [Function("GetDncList")]
    public async Task<HttpResponseData> GetDncList([HttpTrigger(AuthorizationLevel.Function, "get", Route = "dnc-lists/{id:int}")] HttpRequestData req, int id)
    {
        try
        {
            _logger.LogInformation("Retrieving DNC list {DncListId}", id);

            var dncList = await _dbContext.DncLists
                .Include(dl => dl.Campaign)
                .Include(dl => dl.List)
                .FirstOrDefaultAsync(dl => dl.Id == id);

            if (dncList == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync("DNC list not found");
                return notFoundResponse;
            }

            var dncListDto = new DncListDto
            {
                Id = dncList.Id,
                Name = dncList.Name,
                Description = dncList.Description,
                ListType = dncList.ListType,
                IsActive = dncList.IsActive,
                Scope = dncList.Scope,
                TotalNumbers = dncList.TotalNumbers,
                Source = dncList.Source,
                AutoScrubbing = dncList.AutoScrubbing,
                CampaignId = dncList.CampaignId,
                CampaignName = dncList.Campaign?.Name,
                ListId = dncList.ListId,
                ListName = dncList.List?.Name,
                CreatedAt = dncList.CreatedAt,
                UpdatedAt = dncList.UpdatedAt,
                CreatedBy = dncList.CreatedBy,
                UpdatedBy = dncList.UpdatedBy
            };

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(dncListDto);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving DNC list {DncListId}", id);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }

    [Function("CreateDncList")]
    public async Task<HttpResponseData> CreateDncList([HttpTrigger(AuthorizationLevel.Function, "post", Route = "dnc-lists")] HttpRequestData req)
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

            var createDto = JsonSerializer.Deserialize<CreateDncListDto>(body, _jsonOptions);
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
                await badRequest.WriteStringAsync("DNC list name is required");
                return badRequest;
            }

            // Validate optional foreign keys
            if (createDto.CampaignId.HasValue)
            {
                var campaignExists = await _dbContext.Campaigns.AnyAsync(c => c.Id == createDto.CampaignId.Value);
                if (!campaignExists)
                {
                    var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFound.WriteStringAsync("Campaign not found");
                    return notFound;
                }
            }

            if (createDto.ListId.HasValue)
            {
                var listExists = await _dbContext.Lists.AnyAsync(l => l.Id == createDto.ListId.Value);
                if (!listExists)
                {
                    var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFound.WriteStringAsync("List not found");
                    return notFound;
                }
            }

            var dncList = new DncList
            {
                Name = createDto.Name,
                Description = createDto.Description,
                ListType = createDto.ListType,
                IsActive = createDto.IsActive,
                Scope = createDto.Scope,
                TotalNumbers = 0,
                Source = createDto.Source,
                AutoScrubbing = createDto.AutoScrubbing,
                CampaignId = createDto.CampaignId,
                ListId = createDto.ListId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System", // TODO: Get from auth context
                UpdatedBy = "System"
            };

            _dbContext.DncLists.Add(dncList);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Created DNC list {DncListId} with name {Name}", dncList.Id, dncList.Name);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(new { id = dncList.Id, message = "DNC list created successfully" });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating DNC list");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }

    #endregion

    #region DNC Numbers

    [Function("GetDncNumbers")]
    public async Task<HttpResponseData> GetDncNumbers([HttpTrigger(AuthorizationLevel.Function, "get", Route = "dnc-lists/{dncListId:int}/numbers")] HttpRequestData req, int dncListId)
    {
        try
        {
            _logger.LogInformation("Retrieving DNC numbers for list {DncListId}", dncListId);

            // Verify DNC list exists
            var dncListExists = await _dbContext.DncLists.AnyAsync(dl => dl.Id == dncListId);
            if (!dncListExists)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync("DNC list not found");
                return notFound;
            }

            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            var page = int.TryParse(query["page"], out var p) ? p : 1;
            var pageSize = int.TryParse(query["pageSize"], out var ps) ? Math.Min(ps, 1000) : 50;

            var totalCount = await _dbContext.DncNumbers
                .Where(dn => dn.DncListId == dncListId)
                .CountAsync();

            var dncNumbers = await _dbContext.DncNumbers
                .Include(dn => dn.DncList)
                .Where(dn => dn.DncListId == dncListId)
                .OrderBy(dn => dn.PhoneNumber)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(dn => new DncNumberDto
                {
                    Id = dn.Id,
                    DncListId = dn.DncListId,
                    DncListName = dn.DncList!.Name,
                    PhoneNumber = dn.PhoneNumber,
                    PhoneCode = dn.PhoneCode,
                    Reason = dn.Reason,
                    AddedAt = dn.AddedAt,
                    Notes = dn.Notes,
                    AddedBy = dn.AddedBy
                })
                .ToListAsync();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                numbers = dncNumbers,
                pagination = new
                {
                    currentPage = page,
                    pageSize,
                    totalCount,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                }
            });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving DNC numbers for list {DncListId}", dncListId);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }

    [Function("AddDncNumber")]
    public async Task<HttpResponseData> AddDncNumber([HttpTrigger(AuthorizationLevel.Function, "post", Route = "dnc-lists/{dncListId:int}/numbers")] HttpRequestData req, int dncListId)
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

            var createDto = JsonSerializer.Deserialize<CreateDncNumberDto>(body, _jsonOptions);
            if (createDto == null)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Invalid request body");
                return badRequest;
            }

            // Verify DNC list exists
            var dncList = await _dbContext.DncLists.FindAsync(dncListId);
            if (dncList == null)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync("DNC list not found");
                return notFound;
            }

            // Validate required fields
            if (string.IsNullOrEmpty(createDto.PhoneNumber))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Phone number is required");
                return badRequest;
            }

            // Check if number already exists in this list
            var existingNumber = await _dbContext.DncNumbers
                .AnyAsync(dn => dn.DncListId == dncListId && dn.PhoneNumber == createDto.PhoneNumber);

            if (existingNumber)
            {
                var conflict = req.CreateResponse(HttpStatusCode.Conflict);
                await conflict.WriteStringAsync("Phone number already exists in this DNC list");
                return conflict;
            }

            var dncNumber = new DncNumber
            {
                DncListId = dncListId,
                PhoneNumber = createDto.PhoneNumber,
                PhoneCode = createDto.PhoneCode,
                Reason = createDto.Reason,
                AddedAt = DateTime.UtcNow,
                Notes = createDto.Notes,
                AddedBy = "System" // TODO: Get from auth context
            };

            _dbContext.DncNumbers.Add(dncNumber);

            // Update total count on DNC list
            dncList.TotalNumbers++;
            dncList.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Added DNC number {PhoneNumber} to list {DncListId}", createDto.PhoneNumber, dncListId);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(new { id = dncNumber.Id, message = "DNC number added successfully" });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding DNC number to list {DncListId}", dncListId);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }

    [Function("CheckDncStatus")]
    public async Task<HttpResponseData> CheckDncStatus([HttpTrigger(AuthorizationLevel.Function, "post", Route = "dnc/check")] HttpRequestData req)
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

            var checkDto = JsonSerializer.Deserialize<DncCheckDto>(body, _jsonOptions);
            if (checkDto == null)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Invalid request body");
                return badRequest;
            }

            if (string.IsNullOrEmpty(checkDto.PhoneNumber))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Phone number is required");
                return badRequest;
            }

            var dncQuery = _dbContext.DncNumbers
                .Include(dn => dn.DncList)
                .Where(dn => dn.PhoneNumber == checkDto.PhoneNumber);

            // Apply scope filters
            if (checkDto.CampaignId.HasValue)
            {
                dncQuery = dncQuery.Where(dn =>
                    dn.DncList.Scope == "SYSTEM_WIDE" ||
                    (dn.DncList.Scope == "CAMPAIGN" && dn.DncList.CampaignId == checkDto.CampaignId.Value));
            }

            if (checkDto.ListId.HasValue)
            {
                dncQuery = dncQuery.Where(dn =>
                    dn.DncList.Scope == "SYSTEM_WIDE" ||
                    (dn.DncList.Scope == "LIST" && dn.DncList.ListId == checkDto.ListId.Value));
            }

            var matches = await dncQuery
                .Select(dn => new DncMatchDto
                {
                    DncListId = dn.DncListId,
                    DncListName = dn.DncList.Name,
                    Reason = dn.Reason,
                    AddedAt = dn.AddedAt
                })
                .ToListAsync();

            var result = new DncCheckResultDto
            {
                PhoneNumber = checkDto.PhoneNumber,
                IsOnDnc = matches.Any(),
                Matches = matches
            };

            _logger.LogInformation("DNC check for {PhoneNumber}: {IsOnDnc} ({MatchCount} matches)",
                checkDto.PhoneNumber, result.IsOnDnc, matches.Count);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking DNC status");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }

    #endregion
}
