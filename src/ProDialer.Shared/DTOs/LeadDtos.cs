namespace ProDialer.Shared.DTOs;

public class CreateLeadDto
{
    public int ListId { get; set; }
    
    // Core contact information
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName { get; set; }
    public string? Company { get; set; }
    public string? Title { get; set; }
    public string? MiddleInitial { get; set; }
    
    // Phone numbers and validation
    public string PrimaryPhone { get; set; } = string.Empty;
    public string PhoneCode { get; set; } = "1";
    public string? PhoneNumberRaw { get; set; }
    public string PhoneValidationStatus { get; set; } = "UNVALIDATED";
    public string? PhoneCarrier { get; set; }
    public string? PhoneType { get; set; }
    public string? SecondaryPhone { get; set; }
    public string? MobilePhone { get; set; }
    public string? WorkPhone { get; set; }
    public string? HomePhone { get; set; }
    public string? AlternatePhones { get; set; }
    
    // VICIdial integration fields
    public string? VendorLeadCode { get; set; }
    public string? SourceId { get; set; }
    
    // Email addresses
    public string? PrimaryEmail { get; set; }
    public string? SecondaryEmail { get; set; }
    
    // Address information
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? AddressLine3 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Province { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? TimeZone { get; set; }
    
    // Lead management
    public int Priority { get; set; } = 5;
    public int Rank { get; set; } = 0;
    public int QualityScore { get; set; } = 50;
    public string LifecycleStage { get; set; } = "NEW";
    public string? Owner { get; set; }
    public int? EntryListId { get; set; }
    public decimal GmtOffset { get; set; } = 0;
    public decimal GmtOffsetNow { get; set; } = 0;
    
    // Security and verification
    public string? SecurityPhrase { get; set; }
    
    // Compliance and flags
    public string? ComplianceFlags { get; set; }
    public string? CallbackAppointment { get; set; }
    
    // Demographics
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? MaritalStatus { get; set; }
    public string? Income { get; set; }
    public string? Education { get; set; }
    public string? Interests { get; set; }
    
    // Lead attribution and tracking
    public string? Source { get; set; }
    public string? SourceCampaign { get; set; }
    public decimal? LeadValue { get; set; }
    public decimal? ExpectedRevenue { get; set; }
    public string? Tags { get; set; }
    public string? CustomFields { get; set; }
    public string? ConversionTracking { get; set; }
    public string? InteractionHistory { get; set; }
    public string? ScoringFactors { get; set; }
    public string? AttributionData { get; set; }
    
    // Notes and comments
    public string? Notes { get; set; }
}

public class UpdateLeadDto : CreateLeadDto
{
    public int Id { get; set; }
    
    // Call management fields
    public string Status { get; set; } = "NEW";
    public string? User { get; set; }
    public int CallAttempts { get; set; } = 0;
    public int CalledCount { get; set; } = 0;
    public int RecycleCount { get; set; } = 0;
    public DateTime? LastLocalCallTime { get; set; }
    public bool CalledSinceLastReset { get; set; } = false;
    public DateTime? LastCalledAt { get; set; }
    public DateTime? LastContactedAt { get; set; }
    public DateTime? NextCallAt { get; set; }
    public DateTime? ScheduledCallbackAt { get; set; }
    public DateTime? LastRecycledAt { get; set; }
    public string? LastHandlerAgent { get; set; }
    public string? LastCallOutcome { get; set; }
    public string? Disposition { get; set; }
    
    // Exclusion and opt-out management
    public bool IsExcluded { get; set; }
    public string? ExclusionReason { get; set; }
    public DateTime? ExcludedAt { get; set; }
    public bool HasOptedOut { get; set; }
    public DateTime? OptedOutAt { get; set; }
    
    // Modification tracking
    public DateTime ModifyDate { get; set; }
}

public class LeadDto
{
    public int Id { get; set; }
    public int ListId { get; set; }
    
    // Core contact information
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName { get; set; }
    public string? Company { get; set; }
    public string? Title { get; set; }
    public string? MiddleInitial { get; set; }
    
    // Phone numbers and validation
    public string PrimaryPhone { get; set; } = string.Empty;
    public string PhoneCode { get; set; } = "1";
    public string? PhoneNumberRaw { get; set; }
    public string PhoneValidationStatus { get; set; } = "UNVALIDATED";
    public string? PhoneCarrier { get; set; }
    public string? PhoneType { get; set; }
    public string? SecondaryPhone { get; set; }
    public string? MobilePhone { get; set; }
    public string? WorkPhone { get; set; }
    public string? HomePhone { get; set; }
    public string? AlternatePhones { get; set; }
    
    // VICIdial integration fields
    public string? VendorLeadCode { get; set; }
    public string? SourceId { get; set; }
    
    // Email addresses
    public string? PrimaryEmail { get; set; }
    public string? SecondaryEmail { get; set; }
    
    // Address information
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? AddressLine3 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Province { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? TimeZone { get; set; }
    
    // Call management fields
    public string Status { get; set; } = "NEW";
    public string? User { get; set; }
    public int Priority { get; set; }
    public int Rank { get; set; } = 0;
    public int CallAttempts { get; set; }
    public int CalledCount { get; set; } = 0;
    public int QualityScore { get; set; } = 50;
    public string LifecycleStage { get; set; } = "NEW";
    public int RecycleCount { get; set; } = 0;
    public DateTime? LastLocalCallTime { get; set; }
    public bool CalledSinceLastReset { get; set; } = false;
    public DateTime? LastCalledAt { get; set; }
    public DateTime? LastContactedAt { get; set; }
    public DateTime? NextCallAt { get; set; }
    public DateTime? ScheduledCallbackAt { get; set; }
    public DateTime? LastRecycledAt { get; set; }
    public string? LastHandlerAgent { get; set; }
    public string? LastCallOutcome { get; set; }
    public string? Disposition { get; set; }
    
    // Lead management
    public string? Owner { get; set; }
    public int? EntryListId { get; set; }
    public decimal GmtOffset { get; set; } = 0;
    public decimal GmtOffsetNow { get; set; } = 0;
    
    // Security and verification
    public string? SecurityPhrase { get; set; }
    
    // Compliance and flags
    public string? ComplianceFlags { get; set; }
    public string? CallbackAppointment { get; set; }
    
    // Demographics
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? MaritalStatus { get; set; }
    public string? Income { get; set; }
    public string? Education { get; set; }
    public string? Interests { get; set; }
    
    // Lead attribution and tracking
    public string? Source { get; set; }
    public string? SourceCampaign { get; set; }
    public decimal? LeadValue { get; set; }
    public decimal? ExpectedRevenue { get; set; }
    public string? Tags { get; set; }
    public string? CustomFields { get; set; }
    public string? ConversionTracking { get; set; }
    public string? InteractionHistory { get; set; }
    public string? ScoringFactors { get; set; }
    public string? AttributionData { get; set; }
    
    // Notes and comments
    public string? Notes { get; set; }
    
    // Exclusion and opt-out management
    public bool IsExcluded { get; set; }
    public string? ExclusionReason { get; set; }
    public DateTime? ExcludedAt { get; set; }
    public bool HasOptedOut { get; set; }
    public DateTime? OptedOutAt { get; set; }
    
    // Tracking fields
    public DateTime ModifyDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
    
    // Navigation properties
    public string? ListName { get; set; }
}

public class LeadSummaryDto
{
    public int Id { get; set; }
    public int ListId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName { get; set; }
    public string? Company { get; set; }
    public string PrimaryPhone { get; set; } = string.Empty;
    public string? PrimaryEmail { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? User { get; set; }
    public int Priority { get; set; }
    public int Rank { get; set; } = 0;
    public int CallCount { get; set; }
    public int CalledCount { get; set; } = 0;
    public int QualityScore { get; set; } = 50;
    public string LifecycleStage { get; set; } = "NEW";
    public DateTime? LastCallAttempt { get; set; }
    public DateTime? ScheduledCallbackAt { get; set; }
    public string? Disposition { get; set; }
    public string? Owner { get; set; }
    public bool IsExcluded { get; set; }
    public bool HasOptedOut { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class LeadDetailDto : LeadSummaryDto
{
    // Navigation properties
    public string? ListName { get; set; }
    
    // Extended contact information
    public string? Title { get; set; }
    public string? MiddleInitial { get; set; }
    
    // Extended phone information
    public string PhoneCode { get; set; } = "1";
    public string? PhoneNumberRaw { get; set; }
    public string PhoneValidationStatus { get; set; } = "UNVALIDATED";
    public string? PhoneCarrier { get; set; }
    public string? PhoneType { get; set; }
    public string? SecondaryPhone { get; set; }
    public string? MobilePhone { get; set; }
    public string? WorkPhone { get; set; }
    public string? HomePhone { get; set; }
    public string? AlternatePhones { get; set; }
    
    // VICIdial integration fields
    public string? VendorLeadCode { get; set; }
    public string? SourceId { get; set; }
    
    // Extended email
    public string? SecondaryEmail { get; set; }
    
    // Extended address information
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? AddressLine3 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Province { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? TimeZone { get; set; }
    
    // Extended call management
    public int CallAttempts { get; set; }
    public int RecycleCount { get; set; } = 0;
    public DateTime? LastLocalCallTime { get; set; }
    public bool CalledSinceLastReset { get; set; } = false;
    public DateTime? LastCalledAt { get; set; }
    public DateTime? LastContactedAt { get; set; }
    public DateTime? NextCallAt { get; set; }
    public DateTime? LastRecycledAt { get; set; }
    public string? LastHandlerAgent { get; set; }
    public string? LastCallOutcome { get; set; }
    
    // Lead management
    public int? EntryListId { get; set; }
    public decimal GmtOffset { get; set; } = 0;
    public decimal GmtOffsetNow { get; set; } = 0;
    
    // Security and verification
    public string? SecurityPhrase { get; set; }
    
    // Compliance and flags
    public string? ComplianceFlags { get; set; }
    public string? CallbackAppointment { get; set; }
    
    // Demographics
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? MaritalStatus { get; set; }
    public string? Income { get; set; }
    public string? Education { get; set; }
    public string? Interests { get; set; }
    
    // Lead attribution and tracking
    public string? Source { get; set; }
    public string? SourceCampaign { get; set; }
    public decimal? LeadValue { get; set; }
    public decimal? ExpectedRevenue { get; set; }
    public string? Tags { get; set; }
    public string? CustomFields { get; set; }
    public string? ConversionTracking { get; set; }
    public string? InteractionHistory { get; set; }
    public string? ScoringFactors { get; set; }
    public string? AttributionData { get; set; }
    
    // Notes and exclusion details
    public string? Notes { get; set; }
    public string? ExclusionReason { get; set; }
    public DateTime? ExcludedAt { get; set; }
    public DateTime? OptedOutAt { get; set; }
    
    // Tracking
    public DateTime ModifyDate { get; set; }
    
    // Call history
    public List<CallLogSummaryDto> CallHistory { get; set; } = new();
}

public class BulkLeadImportDto
{
    public int ListId { get; set; }
    public List<CreateLeadDto> Leads { get; set; } = new();
    public bool SkipDuplicates { get; set; } = true;
    public string? DuplicateCheckField { get; set; } = "PrimaryPhone";
}

public class LeadListResponseDto
{
    public List<LeadSummaryDto> Leads { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
