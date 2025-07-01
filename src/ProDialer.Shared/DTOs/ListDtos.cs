namespace ProDialer.Shared.DTOs;

public class CreateListDto
{
    // Core List Properties
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Priority { get; set; } = 5;
    public string CallStrategy { get; set; } = "Sequential"; // Sequential, Random, Priority
    public int? MaxCallAttempts { get; set; }
    public int? MinCallInterval { get; set; }
    
    // Time Zone and Calling Hours Override
    public string? TimeZoneOverride { get; set; }
    public string? CallStartTimeOverride { get; set; }
    public string? CallEndTimeOverride { get; set; }
    public string? AllowedDaysOverride { get; set; }
    
    // List Configuration
    public bool IsActive { get; set; } = true;
    public string Source { get; set; } = "Manual";
    public string? SourceReference { get; set; }
    public string? CustomFieldsSchema { get; set; }
    public string? Tags { get; set; }
    
    // VICIdial Enhancement Properties
    public string? ScriptId { get; set; }
    public string? AgentScriptOverride { get; set; }
    public string? CampaignCallerIdOverride { get; set; }
    public decimal ListMixRatio { get; set; } = 1.0m;
    public string DuplicateCheckMethod { get; set; } = "PHONE";
    public bool CustomFieldsCopy { get; set; } = false;
    public bool CustomFieldsModify { get; set; } = true;
    public bool ResetLeadCalledCount { get; set; } = true;
    public string? PhoneValidationSettings { get; set; }
    public string? ImportExportLog { get; set; }
    public string? PerformanceMetrics { get; set; }
    
    // Answering Machine and Drop Settings
    public string? AnsweringMachineMessage { get; set; }
    public string? DropInGroup { get; set; }
    
    // Web Forms and URLs
    public string? WebFormAddress { get; set; }
    public string? WebFormAddress2 { get; set; }
    public string? WebFormAddress3 { get; set; }
    
    // Reset and Timezone Configuration
    public string? ResetTime { get; set; }
    public string TimezoneMethod { get; set; } = "COUNTRY_AND_AREA_CODE";
    public string? LocalCallTime { get; set; }
    public DateTime? ExpirationDate { get; set; }
    
    // Transfer Configuration
    public string? OutboundCallerId { get; set; }
    public string? TransferConf1 { get; set; }
    public string? TransferConf2 { get; set; }
    public string? TransferConf3 { get; set; }
    public string? TransferConf4 { get; set; }
    public string? TransferConf5 { get; set; }
}

public class UpdateListDto : CreateListDto
{
    public int Id { get; set; }
}

public class ListDto
{
    public int Id { get; set; }
    
    // Core List Properties
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Priority { get; set; }
    public string CallStrategy { get; set; } = "Sequential";
    public int? MaxCallAttempts { get; set; }
    public int? MinCallInterval { get; set; }
    
    // Time Zone and Calling Hours Override
    public string? TimeZoneOverride { get; set; }
    public string? CallStartTimeOverride { get; set; }
    public string? CallEndTimeOverride { get; set; }
    public string? AllowedDaysOverride { get; set; }
    
    // List Configuration
    public bool IsActive { get; set; }
    public string Source { get; set; } = string.Empty;
    public string? SourceReference { get; set; }
    public string? CustomFieldsSchema { get; set; }
    public string? Tags { get; set; }
    
    // Performance Metrics
    public int TotalLeads { get; set; }
    public int CalledLeads { get; set; }
    public int ContactedLeads { get; set; }
    
    // VICIdial Enhancement Properties
    public string? ScriptId { get; set; }
    public string? AgentScriptOverride { get; set; }
    public string? CampaignCallerIdOverride { get; set; }
    public decimal ListMixRatio { get; set; }
    public string DuplicateCheckMethod { get; set; } = string.Empty;
    public bool CustomFieldsCopy { get; set; }
    public bool CustomFieldsModify { get; set; }
    public bool ResetLeadCalledCount { get; set; }
    public string? PhoneValidationSettings { get; set; }
    public string? ImportExportLog { get; set; }
    public string? PerformanceMetrics { get; set; }
    
    // Answering Machine and Drop Settings
    public string? AnsweringMachineMessage { get; set; }
    public string? DropInGroup { get; set; }
    
    // Web Forms and URLs
    public string? WebFormAddress { get; set; }
    public string? WebFormAddress2 { get; set; }
    public string? WebFormAddress3 { get; set; }
    
    // Reset and Timezone Configuration
    public string? ResetTime { get; set; }
    public string TimezoneMethod { get; set; } = string.Empty;
    public string? LocalCallTime { get; set; }
    public DateTime? ExpirationDate { get; set; }
    
    // Transfer Configuration
    public string? OutboundCallerId { get; set; }
    public string? TransferConf1 { get; set; }
    public string? TransferConf2 { get; set; }
    public string? TransferConf3 { get; set; }
    public string? TransferConf4 { get; set; }
    public string? TransferConf5 { get; set; }
    
    // Reset Statistics
    public int ResetsToday { get; set; }
    public DateTime? LastResetAt { get; set; }
    
    // Audit Fields
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}

public class ListSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Performance Metrics
    public int TotalLeads { get; set; }
    public int CalledLeads { get; set; }
    public int ContactedLeads { get; set; }
    public decimal ListMixRatio { get; set; }
    public string DuplicateCheckMethod { get; set; } = string.Empty;
    
    // Reset Statistics
    public int ResetsToday { get; set; }
    public DateTime? LastResetAt { get; set; }
    public DateTime? ExpirationDate { get; set; }
}
