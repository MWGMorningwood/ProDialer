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
/// Azure Functions for managing disposition categories and codes
/// Provides CRUD operations and disposition-specific functionality for VICIdial compatibility
/// </summary>
public class DispositionFunctions
{
    private readonly ProDialerDbContext _dbContext;
    private readonly ILogger<DispositionFunctions> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public DispositionFunctions(ProDialerDbContext dbContext, ILogger<DispositionFunctions> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    #region Disposition Categories

    [Function("GetDispositionCategories")]
    public async Task<HttpResponseData> GetDispositionCategories([HttpTrigger(AuthorizationLevel.Function, "get", Route = "disposition-categories")] HttpRequestData req)
    {
        try
        {
            _logger.LogInformation("Retrieving disposition categories");

            var categories = await _dbContext.DispositionCategories
                .Where(dc => dc.IsActive)
                .OrderBy(dc => dc.DisplayOrder)
                .ThenBy(dc => dc.Name)
                .Select(dc => new DispositionCategoryDto
                {
                    Id = dc.Id,
                    Name = dc.Name,
                    Description = dc.Description,
                    Code = dc.Code,
                    Color = dc.Color,
                    DisplayOrder = dc.DisplayOrder,
                    IsActive = dc.IsActive,
                    CreatedAt = dc.CreatedAt,
                    UpdatedAt = dc.UpdatedAt
                })
                .ToListAsync();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(categories);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving disposition categories");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }

    [Function("GetDispositionCategory")]
    public async Task<HttpResponseData> GetDispositionCategory([HttpTrigger(AuthorizationLevel.Function, "get", Route = "disposition-categories/{id:int}")] HttpRequestData req, int id)
    {
        try
        {
            _logger.LogInformation("Retrieving disposition category {CategoryId}", id);

            var category = await _dbContext.DispositionCategories
                .FirstOrDefaultAsync(dc => dc.Id == id);

            if (category == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync("Disposition category not found");
                return notFoundResponse;
            }

            var categoryDto = new DispositionCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Code = category.Code,
                Color = category.Color,
                DisplayOrder = category.DisplayOrder,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(categoryDto);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving disposition category {CategoryId}", id);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }

    [Function("CreateDispositionCategory")]
    public async Task<HttpResponseData> CreateDispositionCategory([HttpTrigger(AuthorizationLevel.Function, "post", Route = "disposition-categories")] HttpRequestData req)
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

            var createDto = JsonSerializer.Deserialize<CreateDispositionCategoryDto>(body, _jsonOptions);
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
                await badRequest.WriteStringAsync("Category name is required");
                return badRequest;
            }

            if (string.IsNullOrEmpty(createDto.Code))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Category code is required");
                return badRequest;
            }

            // Check for duplicate code
            var existingCategory = await _dbContext.DispositionCategories
                .AnyAsync(dc => dc.Code == createDto.Code);

            if (existingCategory)
            {
                var conflict = req.CreateResponse(HttpStatusCode.Conflict);
                await conflict.WriteStringAsync("A category with this code already exists");
                return conflict;
            }

            var category = new DispositionCategory
            {
                Name = createDto.Name,
                Description = createDto.Description,
                Code = createDto.Code,
                Color = createDto.Color,
                DisplayOrder = createDto.DisplayOrder,
                IsActive = createDto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.DispositionCategories.Add(category);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Created disposition category {CategoryId} with code {Code}", category.Id, category.Code);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(new { id = category.Id, message = "Disposition category created successfully" });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating disposition category");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }

    #endregion

    #region Disposition Codes

    [Function("GetDispositionCodes")]
    public async Task<HttpResponseData> GetDispositionCodes([HttpTrigger(AuthorizationLevel.Function, "get", Route = "disposition-codes")] HttpRequestData req)
    {
        try
        {
            _logger.LogInformation("Retrieving disposition codes");

            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            var categoryIdParam = query["categoryId"];

            var codesQuery = _dbContext.DispositionCodes
                .Include(dc => dc.Category)
                .Where(dc => dc.IsActive);

            // Filter by category if specified
            if (int.TryParse(categoryIdParam, out var categoryId))
            {
                codesQuery = codesQuery.Where(dc => dc.CategoryId == categoryId);
            }

            var codes = await codesQuery
                .OrderBy(dc => dc.DisplayOrder)
                .ThenBy(dc => dc.Name)
                .Select(dc => new DispositionCodeDto
                {
                    Id = dc.Id,
                    Name = dc.Name,
                    Description = dc.Description,
                    Code = dc.Code,
                    CategoryId = dc.CategoryId,
                    CategoryName = dc.Category!.Name,
                    IsContact = dc.IsContact,
                    IsSale = dc.IsSale,
                    ShouldRecycle = dc.ShouldRecycle,
                    RecycleDelayHours = dc.RecycleDelayHours,
                    RequiresCallback = dc.RequiresCallback,
                    RequiredFields = dc.RequiredFields,
                    AutoActions = dc.AutoActions,
                    HotKey = dc.HotKey,
                    DisplayOrder = dc.DisplayOrder,
                    IsActive = dc.IsActive,
                    CreatedAt = dc.CreatedAt,
                    UpdatedAt = dc.UpdatedAt
                })
                .ToListAsync();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(codes);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving disposition codes");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }

    [Function("CreateDispositionCode")]
    public async Task<HttpResponseData> CreateDispositionCode([HttpTrigger(AuthorizationLevel.Function, "post", Route = "disposition-codes")] HttpRequestData req)
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

            var createDto = JsonSerializer.Deserialize<CreateDispositionCodeDto>(body, _jsonOptions);
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
                await badRequest.WriteStringAsync("Code name is required");
                return badRequest;
            }

            if (string.IsNullOrEmpty(createDto.Code))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Code is required");
                return badRequest;
            }

            // Validate category exists
            var categoryExists = await _dbContext.DispositionCategories
                .AnyAsync(dc => dc.Id == createDto.CategoryId);

            if (!categoryExists)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync("Disposition category not found");
                return notFound;
            }

            // Check for duplicate code
            var existingCode = await _dbContext.DispositionCodes
                .AnyAsync(dc => dc.Code == createDto.Code);

            if (existingCode)
            {
                var conflict = req.CreateResponse(HttpStatusCode.Conflict);
                await conflict.WriteStringAsync("A disposition code with this code already exists");
                return conflict;
            }

            var dispositionCode = new DispositionCode
            {
                Name = createDto.Name,
                Description = createDto.Description,
                Code = createDto.Code,
                CategoryId = createDto.CategoryId,
                IsContact = createDto.IsContact,
                IsSale = createDto.IsSale,
                ShouldRecycle = createDto.ShouldRecycle,
                RecycleDelayHours = createDto.RecycleDelayHours,
                RequiresCallback = createDto.RequiresCallback,
                RequiredFields = createDto.RequiredFields,
                AutoActions = createDto.AutoActions,
                HotKey = createDto.HotKey,
                DisplayOrder = createDto.DisplayOrder,
                IsActive = createDto.IsActive,
                UsageCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.DispositionCodes.Add(dispositionCode);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Created disposition code {CodeId} with code {Code}", dispositionCode.Id, dispositionCode.Code);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(new { id = dispositionCode.Id, message = "Disposition code created successfully" });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating disposition code");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }

    #endregion
}
