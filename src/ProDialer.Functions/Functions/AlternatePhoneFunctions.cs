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
/// Azure Functions for managing alternate phone numbers for leads
/// Provides CRUD operations and phone validation functionality for VICIdial compatibility
/// </summary>
public class AlternatePhoneFunctions
{
    private readonly ProDialerDbContext _dbContext;
    private readonly ILogger<AlternatePhoneFunctions> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public AlternatePhoneFunctions(ProDialerDbContext dbContext, ILogger<AlternatePhoneFunctions> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    [Function("GetAlternatePhones")]
    public async Task<HttpResponseData> GetAlternatePhones([HttpTrigger(AuthorizationLevel.Function, "get", Route = "leads/{leadId:int}/alternate-phones")] HttpRequestData req, int leadId)
    {
        try
        {
            _logger.LogInformation("Retrieving alternate phones for lead {LeadId}", leadId);

            // Verify lead exists
            var leadExists = await _dbContext.Leads.AnyAsync(l => l.Id == leadId);
            if (!leadExists)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync("Lead not found");
                return notFound;
            }

            var alternatePhones = await _dbContext.AlternatePhones
                .Include(ap => ap.Lead)
                .Where(ap => ap.LeadId == leadId && ap.IsActive)
                .OrderBy(ap => ap.Priority)
                .ThenBy(ap => ap.PhoneType)
                .Select(ap => new AlternatePhoneDto
                {
                    Id = ap.Id,
                    LeadId = ap.LeadId,
                    LeadName = ap.Lead != null ? $"{ap.Lead.FirstName} {ap.Lead.LastName}".Trim() : null,
                    PhoneNumber = ap.PhoneNumber,
                    PhoneCode = ap.PhoneCode,
                    PhoneType = ap.PhoneType,
                    Priority = ap.Priority,
                    Status = ap.Status,
                    CallAttempts = ap.CallAttempts,
                    LastCallOutcome = ap.LastCallOutcome,
                    LastCalledAt = ap.LastCalledAt,
                    IsValidated = ap.IsValidated,
                    ValidationResult = ap.ValidationResult,
                    Carrier = ap.Carrier,
                    LineType = ap.LineType,
                    TimeZone = ap.TimeZone,
                    BestCallTime = ap.BestCallTime,
                    Notes = ap.Notes,
                    IsActive = ap.IsActive,
                    CreatedAt = ap.CreatedAt,
                    UpdatedAt = ap.UpdatedAt,
                    CreatedBy = ap.CreatedBy,
                    UpdatedBy = ap.UpdatedBy
                })
                .ToListAsync();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(alternatePhones);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving alternate phones for lead {LeadId}", leadId);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }

    [Function("GetAlternatePhone")]
    public async Task<HttpResponseData> GetAlternatePhone([HttpTrigger(AuthorizationLevel.Function, "get", Route = "alternate-phones/{id:int}")] HttpRequestData req, int id)
    {
        try
        {
            _logger.LogInformation("Retrieving alternate phone {AlternatePhoneId}", id);

            var alternatePhone = await _dbContext.AlternatePhones
                .Include(ap => ap.Lead)
                .FirstOrDefaultAsync(ap => ap.Id == id);

            if (alternatePhone == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync("Alternate phone not found");
                return notFoundResponse;
            }

            var alternatePhoneDto = new AlternatePhoneDto
            {
                Id = alternatePhone.Id,
                LeadId = alternatePhone.LeadId,
                LeadName = alternatePhone.Lead != null ? $"{alternatePhone.Lead.FirstName} {alternatePhone.Lead.LastName}".Trim() : null,
                PhoneNumber = alternatePhone.PhoneNumber,
                PhoneCode = alternatePhone.PhoneCode,
                PhoneType = alternatePhone.PhoneType,
                Priority = alternatePhone.Priority,
                Status = alternatePhone.Status,
                CallAttempts = alternatePhone.CallAttempts,
                LastCallOutcome = alternatePhone.LastCallOutcome,
                LastCalledAt = alternatePhone.LastCalledAt,
                IsValidated = alternatePhone.IsValidated,
                ValidationResult = alternatePhone.ValidationResult,
                Carrier = alternatePhone.Carrier,
                LineType = alternatePhone.LineType,
                TimeZone = alternatePhone.TimeZone,
                BestCallTime = alternatePhone.BestCallTime,
                Notes = alternatePhone.Notes,
                IsActive = alternatePhone.IsActive,
                CreatedAt = alternatePhone.CreatedAt,
                UpdatedAt = alternatePhone.UpdatedAt,
                CreatedBy = alternatePhone.CreatedBy,
                UpdatedBy = alternatePhone.UpdatedBy
            };

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(alternatePhoneDto);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving alternate phone {AlternatePhoneId}", id);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }

    [Function("CreateAlternatePhone")]
    public async Task<HttpResponseData> CreateAlternatePhone([HttpTrigger(AuthorizationLevel.Function, "post", Route = "leads/{leadId:int}/alternate-phones")] HttpRequestData req, int leadId)
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

            var createDto = JsonSerializer.Deserialize<CreateAlternatePhoneDto>(body, _jsonOptions);
            if (createDto == null)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Invalid request body");
                return badRequest;
            }

            // Verify lead exists
            var leadExists = await _dbContext.Leads.AnyAsync(l => l.Id == leadId);
            if (!leadExists)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync("Lead not found");
                return notFound;
            }

            // Validate required fields
            if (string.IsNullOrEmpty(createDto.PhoneNumber))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Phone number is required");
                return badRequest;
            }

            // Check for duplicate phone number for this lead
            var existingPhone = await _dbContext.AlternatePhones
                .AnyAsync(ap => ap.LeadId == leadId && ap.PhoneNumber == createDto.PhoneNumber && ap.IsActive);

            if (existingPhone)
            {
                var conflict = req.CreateResponse(HttpStatusCode.Conflict);
                await conflict.WriteStringAsync("This phone number already exists for this lead");
                return conflict;
            }

            var alternatePhone = new AlternatePhone
            {
                LeadId = leadId,
                PhoneNumber = createDto.PhoneNumber,
                PhoneCode = createDto.PhoneCode,
                PhoneType = createDto.PhoneType,
                Priority = createDto.Priority,
                Status = createDto.Status,
                CallAttempts = 0,
                IsValidated = createDto.IsValidated,
                ValidationResult = createDto.ValidationResult,
                Carrier = createDto.Carrier,
                LineType = createDto.LineType,
                TimeZone = createDto.TimeZone,
                BestCallTime = createDto.BestCallTime,
                Notes = createDto.Notes,
                IsActive = createDto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System", // TODO: Get from auth context
                UpdatedBy = "System"
            };

            _dbContext.AlternatePhones.Add(alternatePhone);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Created alternate phone {AlternatePhoneId} for lead {LeadId}", alternatePhone.Id, leadId);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(new { id = alternatePhone.Id, message = "Alternate phone created successfully" });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating alternate phone for lead {LeadId}", leadId);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }

    [Function("UpdateAlternatePhone")]
    public async Task<HttpResponseData> UpdateAlternatePhone([HttpTrigger(AuthorizationLevel.Function, "put", Route = "alternate-phones/{id:int}")] HttpRequestData req, int id)
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

            var updateDto = JsonSerializer.Deserialize<UpdateAlternatePhoneDto>(body, _jsonOptions);
            if (updateDto == null)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Invalid request body");
                return badRequest;
            }

            var alternatePhone = await _dbContext.AlternatePhones.FindAsync(id);
            if (alternatePhone == null)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync("Alternate phone not found");
                return notFound;
            }

            // Validate required fields
            if (string.IsNullOrEmpty(updateDto.PhoneNumber))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Phone number is required");
                return badRequest;
            }

            // Check for duplicate phone number for this lead (excluding current record)
            var existingPhone = await _dbContext.AlternatePhones
                .AnyAsync(ap => ap.LeadId == alternatePhone.LeadId && 
                              ap.PhoneNumber == updateDto.PhoneNumber && 
                              ap.Id != id && 
                              ap.IsActive);

            if (existingPhone)
            {
                var conflict = req.CreateResponse(HttpStatusCode.Conflict);
                await conflict.WriteStringAsync("This phone number already exists for this lead");
                return conflict;
            }

            // Update properties
            alternatePhone.PhoneNumber = updateDto.PhoneNumber;
            alternatePhone.PhoneCode = updateDto.PhoneCode;
            alternatePhone.PhoneType = updateDto.PhoneType;
            alternatePhone.Priority = updateDto.Priority;
            alternatePhone.Status = updateDto.Status;
            alternatePhone.IsValidated = updateDto.IsValidated;
            alternatePhone.ValidationResult = updateDto.ValidationResult;
            alternatePhone.Carrier = updateDto.Carrier;
            alternatePhone.LineType = updateDto.LineType;
            alternatePhone.TimeZone = updateDto.TimeZone;
            alternatePhone.BestCallTime = updateDto.BestCallTime;
            alternatePhone.Notes = updateDto.Notes;
            alternatePhone.IsActive = updateDto.IsActive;
            alternatePhone.UpdatedAt = DateTime.UtcNow;
            alternatePhone.UpdatedBy = "System"; // TODO: Get from auth context

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Updated alternate phone {AlternatePhoneId}", id);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new { message = "Alternate phone updated successfully" });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating alternate phone {AlternatePhoneId}", id);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }

    [Function("DeleteAlternatePhone")]
    public async Task<HttpResponseData> DeleteAlternatePhone([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "alternate-phones/{id:int}")] HttpRequestData req, int id)
    {
        try
        {
            var alternatePhone = await _dbContext.AlternatePhones.FindAsync(id);
            if (alternatePhone == null)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync("Alternate phone not found");
                return notFound;
            }

            // Soft delete by setting IsActive to false
            alternatePhone.IsActive = false;
            alternatePhone.UpdatedAt = DateTime.UtcNow;
            alternatePhone.UpdatedBy = "System"; // TODO: Get from auth context

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Deleted alternate phone {AlternatePhoneId}", id);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new { message = "Alternate phone deleted successfully" });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting alternate phone {AlternatePhoneId}", id);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }

    [Function("ValidatePhone")]
    public async Task<HttpResponseData> ValidatePhone([HttpTrigger(AuthorizationLevel.Function, "post", Route = "alternate-phones/{id:int}/validate")] HttpRequestData req, int id)
    {
        try
        {
            var alternatePhone = await _dbContext.AlternatePhones.FindAsync(id);
            if (alternatePhone == null)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync("Alternate phone not found");
                return notFound;
            }

            // TODO: Implement actual phone validation service integration
            // For now, simulate validation
            var validationResult = new PhoneValidationResultDto
            {
                AlternatePhoneId = id,
                PhoneNumber = alternatePhone.PhoneNumber,
                IsValid = true, // Mock validation
                ValidationResult = "VALID",
                Carrier = "Mock Carrier",
                LineType = "MOBILE",
                TimeZone = "UTC-5",
                ValidatedAt = DateTime.UtcNow
            };

            // Update alternate phone with validation results
            alternatePhone.IsValidated = validationResult.IsValid;
            alternatePhone.ValidationResult = validationResult.ValidationResult;
            alternatePhone.Carrier = validationResult.Carrier;
            alternatePhone.LineType = validationResult.LineType;
            alternatePhone.TimeZone = validationResult.TimeZone;
            alternatePhone.UpdatedAt = DateTime.UtcNow;
            alternatePhone.UpdatedBy = "System";

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Validated alternate phone {AlternatePhoneId} with result {ValidationResult}", 
                id, validationResult.ValidationResult);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(validationResult);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating alternate phone {AlternatePhoneId}", id);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }
}
