namespace ProDialer.Shared.DTOs;

// Alternate Phone DTOs
public class CreateAlternatePhoneDto
{
    public int LeadId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string PhoneCode { get; set; } = "1";
    public string PhoneType { get; set; } = "OTHER";
    public int Priority { get; set; } = 1;
    public string Status { get; set; } = "ACTIVE";
    public bool IsValidated { get; set; } = false;
    public string ValidationResult { get; set; } = "UNKNOWN";
    public string? Carrier { get; set; }
    public string? LineType { get; set; }
    public string? TimeZone { get; set; }
    public string? BestCallTime { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateAlternatePhoneDto : CreateAlternatePhoneDto
{
    public int Id { get; set; }
}

public class AlternatePhoneDto
{
    public int Id { get; set; }
    public int LeadId { get; set; }
    public string? LeadName { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string PhoneCode { get; set; } = "1";
    public string PhoneType { get; set; } = "OTHER";
    public int Priority { get; set; }
    public string Status { get; set; } = "ACTIVE";
    public int CallAttempts { get; set; }
    public string? LastCallOutcome { get; set; }
    public DateTime? LastCalledAt { get; set; }
    public bool IsValidated { get; set; }
    public string ValidationResult { get; set; } = "UNKNOWN";
    public string? Carrier { get; set; }
    public string? LineType { get; set; }
    public string? TimeZone { get; set; }
    public string? BestCallTime { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}

public class AlternatePhoneSummaryDto
{
    public int Id { get; set; }
    public int LeadId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string PhoneType { get; set; } = "OTHER";
    public int Priority { get; set; }
    public string Status { get; set; } = "ACTIVE";
    public int CallAttempts { get; set; }
    public bool IsValidated { get; set; }
    public bool IsActive { get; set; }
}

public class PhoneValidationDto
{
    public int AlternatePhoneId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public bool ForceRevalidation { get; set; } = false;
}

public class PhoneValidationResultDto
{
    public int AlternatePhoneId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public string ValidationResult { get; set; } = "UNKNOWN";
    public string? Carrier { get; set; }
    public string? LineType { get; set; }
    public string? TimeZone { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ValidatedAt { get; set; }
}
