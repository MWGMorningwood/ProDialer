namespace ProDialer.Shared.DTOs;

public class CreateLeadDto
{
    public int ListId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName { get; set; }
    public string? Company { get; set; }
    public string? Title { get; set; }
    public string PrimaryPhone { get; set; } = string.Empty;
    public string? SecondaryPhone { get; set; }
    public string? MobilePhone { get; set; }
    public string? WorkPhone { get; set; }
    public string? HomePhone { get; set; }
    public string? PrimaryEmail { get; set; }
    public string? SecondaryEmail { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? TimeZone { get; set; }
    public int Priority { get; set; } = 5;
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? MaritalStatus { get; set; }
    public string? Income { get; set; }
    public string? Education { get; set; }
    public string? Interests { get; set; }
    public string? Source { get; set; }
    public string? SourceCampaign { get; set; }
    public decimal? LeadValue { get; set; }
    public decimal? ExpectedRevenue { get; set; }
    public string? Tags { get; set; }
    public string? CustomFields { get; set; }
    public string? Notes { get; set; }
}

public class UpdateLeadDto : CreateLeadDto
{
    public int Id { get; set; }
    public string Status { get; set; } = "New";
    public DateTime? ScheduledCallbackAt { get; set; }
    public string? Disposition { get; set; }
    public bool IsExcluded { get; set; }
    public string? ExclusionReason { get; set; }
    public bool HasOptedOut { get; set; }
}

public class LeadDto
{
    public int Id { get; set; }
    public int ListId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName { get; set; }
    public string? Company { get; set; }
    public string? Title { get; set; }
    public string PrimaryPhone { get; set; } = string.Empty;
    public string? SecondaryPhone { get; set; }
    public string? MobilePhone { get; set; }
    public string? WorkPhone { get; set; }
    public string? HomePhone { get; set; }
    public string? PrimaryEmail { get; set; }
    public string? SecondaryEmail { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? TimeZone { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Priority { get; set; }
    public int CallAttempts { get; set; }
    public DateTime? LastCalledAt { get; set; }
    public DateTime? LastContactedAt { get; set; }
    public DateTime? NextCallAt { get; set; }
    public DateTime? ScheduledCallbackAt { get; set; }
    public string? LastHandlerAgent { get; set; }
    public string? Notes { get; set; }
    public string? LastCallOutcome { get; set; }
    public string? Disposition { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? MaritalStatus { get; set; }
    public string? Income { get; set; }
    public string? Education { get; set; }
    public string? Interests { get; set; }
    public string? Source { get; set; }
    public string? SourceCampaign { get; set; }
    public decimal? LeadValue { get; set; }
    public decimal? ExpectedRevenue { get; set; }
    public string? Tags { get; set; }
    public string? CustomFields { get; set; }
    public bool IsExcluded { get; set; }
    public string? ExclusionReason { get; set; }
    public DateTime? ExcludedAt { get; set; }
    public bool HasOptedOut { get; set; }
    public DateTime? OptedOutAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
    
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
    public int Priority { get; set; }
    public int CallCount { get; set; }
    public DateTime? LastCallAttempt { get; set; }
    public DateTime? ScheduledCallbackAt { get; set; }
    public string? Disposition { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class LeadDetailDto : LeadSummaryDto
{
    public string? ListName { get; set; }
    public string? Title { get; set; }
    public string? SecondaryPhone { get; set; }
    public string? MobilePhone { get; set; }
    public string? WorkPhone { get; set; }
    public string? HomePhone { get; set; }
    public string? SecondaryEmail { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? TimeZone { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? MaritalStatus { get; set; }
    public string? Income { get; set; }
    public string? Education { get; set; }
    public string? Interests { get; set; }
    public string? Source { get; set; }
    public string? SourceCampaign { get; set; }
    public decimal? LeadValue { get; set; }
    public decimal? ExpectedRevenue { get; set; }
    public string? Tags { get; set; }
    public string? CustomFields { get; set; }
    public string? Notes { get; set; }
    public bool IsExcluded { get; set; }
    public string? ExclusionReason { get; set; }
    public bool HasOptedOut { get; set; }
    public List<CallLogSummaryDto> CallHistory { get; set; } = new();
}

public class CallLogSummaryDto
{
    public int Id { get; set; }
    public string CallDirection { get; set; } = string.Empty;
    public string CallStatus { get; set; } = string.Empty;
    public int CallDuration { get; set; }
    public string? Disposition { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
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
