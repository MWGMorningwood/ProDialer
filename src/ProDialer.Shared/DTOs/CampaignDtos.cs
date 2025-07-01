namespace ProDialer.Shared.DTOs;

public class CreateCampaignDto
{
    // Core Campaign Properties
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DialMethod { get; set; } = "MANUAL";
    public decimal DialingRatio { get; set; } = 1.0m;
    public bool ApplyRatioToIdleAgentsOnly { get; set; } = true;
    public decimal AdaptiveMaxLevel { get; set; } = 2.0m;
    public int DialTimeout { get; set; } = 30;
    public int MaxConcurrentCalls { get; set; } = 50;
    
    // Dial Prefixes and Extensions
    public string? DialPrefix { get; set; }
    public string? ManualDialPrefix { get; set; }
    public string? ThreeWayDialPrefix { get; set; }
    public string? ParkExtension { get; set; }
    public string? ParkFileName { get; set; }
    public string? VoicemailExtension { get; set; }
    public string? AnsweringMachineExtension { get; set; }
    
    // Agent Selection and Routing
    public bool AvailableOnlyRatioTally { get; set; } = false;
    public string AvailableOnlyTallyThreshold { get; set; } = "100";
    public string NextAgentCall { get; set; } = "longest_wait_time";
    
    // Call Handling and Features
    public bool AlternateNumberDialing { get; set; } = false;
    public bool ScheduledCallbacks { get; set; } = true;
    public bool AllowInbound { get; set; } = false;
    public bool ForceResetHopper { get; set; } = false;
    public bool HopperDuplicateCheck { get; set; } = true;
    public string GetCallLaunch { get; set; } = "NONE";
    
    // Time Restrictions
    public string TimeZone { get; set; } = "UTC";
    public string CallStartTime { get; set; } = "08:00";
    public string CallEndTime { get; set; } = "20:00";
    public string AllowedDaysOfWeek { get; set; } = "Monday,Tuesday,Wednesday,Thursday,Friday";
    public bool RespectLeadTimeZone { get; set; } = true;
    
    // Call Attempt Configuration
    public int MinCallInterval { get; set; } = 60;
    public int MaxCallAttempts { get; set; } = 3;
    public int CallAttemptDelay { get; set; } = 15;
    
    // Answering Machine Detection
    public bool EnableAnsweringMachineDetection { get; set; } = true;
    public string AnsweringMachineAction { get; set; } = "Hangup";
    public string? AnsweringMachineMessage { get; set; }
    
    // Lead Processing
    public string LeadOrder { get; set; } = "random";
    public bool RandomizeLeadOrder { get; set; } = false;
    public string? LeadOrderSecondary { get; set; }
    public int HopperLevel { get; set; } = 100;
    
    // Transfer Configuration
    public string? OutboundCallerId { get; set; }
    public string? TransferConf1 { get; set; }
    public string? TransferConf2 { get; set; }
    public string? TransferConf3 { get; set; }
    public string? TransferConf4 { get; set; }
    public string? TransferConf5 { get; set; }
    public string? XferConfADtmf { get; set; }
    public string? XferConfANumber { get; set; }
    public string? XferConfBDtmf { get; set; }
    public string? XferConfBNumber { get; set; }
    
    // Advanced Features
    public string? ContainerEntry { get; set; }
    public string? ManualDialFilter { get; set; }
    public string? DispoCallUrl { get; set; }
    public string? WebForm1 { get; set; }
    public string? WebForm2 { get; set; }
    public string? WebForm3 { get; set; }
    public int WrapupSeconds { get; set; } = 0;
    public string DialStatuses { get; set; } = "READY,QUEUE";
    public string? LeadFilterId { get; set; }
    public int DropCallTimer { get; set; } = 8;
    public string? SafeHarborMessage { get; set; }
    public decimal MaxDropPercentage { get; set; } = 3.0m;
    
    // Campaign Settings
    public bool IsActive { get; set; } = false;
    public int Priority { get; set; } = 5;
    public string? LocationRestrictions { get; set; }
    public bool EnableCallRecording { get; set; } = false;
    public string? CustomFields { get; set; }
    
    // Lead Recycling
    public string? RecyclingRules { get; set; }
    public bool EnableAutoRecycling { get; set; } = false;
    public int MaxRecycleCount { get; set; } = 3;
}

public class UpdateCampaignDto
{
    public int Id { get; set; }
    
    // Core Campaign Properties
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DialMethod { get; set; } = "MANUAL";
    public decimal DialingRatio { get; set; } = 1.0m;
    public bool ApplyRatioToIdleAgentsOnly { get; set; } = true;
    public decimal AdaptiveMaxLevel { get; set; } = 2.0m;
    public int DialTimeout { get; set; } = 30;
    public int MaxConcurrentCalls { get; set; } = 50;
    
    // Dial Prefixes and Extensions
    public string? DialPrefix { get; set; }
    public string? ManualDialPrefix { get; set; }
    public string? ThreeWayDialPrefix { get; set; }
    public string? ParkExtension { get; set; }
    public string? ParkFileName { get; set; }
    public string? VoicemailExtension { get; set; }
    public string? AnsweringMachineExtension { get; set; }
    
    // Agent Selection and Routing
    public bool AvailableOnlyRatioTally { get; set; } = false;
    public string AvailableOnlyTallyThreshold { get; set; } = "100";
    public string NextAgentCall { get; set; } = "longest_wait_time";
    
    // Call Handling and Features
    public bool AlternateNumberDialing { get; set; } = false;
    public bool ScheduledCallbacks { get; set; } = true;
    public bool AllowInbound { get; set; } = false;
    public bool ForceResetHopper { get; set; } = false;
    public bool HopperDuplicateCheck { get; set; } = true;
    public string GetCallLaunch { get; set; } = "NONE";
    
    // Time Restrictions
    public string TimeZone { get; set; } = "UTC";
    public string CallStartTime { get; set; } = "08:00";
    public string CallEndTime { get; set; } = "20:00";
    public string AllowedDaysOfWeek { get; set; } = "Monday,Tuesday,Wednesday,Thursday,Friday";
    public bool RespectLeadTimeZone { get; set; } = true;
    
    // Call Attempt Configuration
    public int MinCallInterval { get; set; } = 60;
    public int MaxCallAttempts { get; set; } = 3;
    public int CallAttemptDelay { get; set; } = 15;
    
    // Answering Machine Detection
    public bool EnableAnsweringMachineDetection { get; set; } = true;
    public string AnsweringMachineAction { get; set; } = "Hangup";
    public string? AnsweringMachineMessage { get; set; }
    
    // Lead Processing
    public string LeadOrder { get; set; } = "random";
    public bool RandomizeLeadOrder { get; set; } = false;
    public string? LeadOrderSecondary { get; set; }
    public int HopperLevel { get; set; } = 100;
    
    // Transfer Configuration
    public string? OutboundCallerId { get; set; }
    public string? TransferConf1 { get; set; }
    public string? TransferConf2 { get; set; }
    public string? TransferConf3 { get; set; }
    public string? TransferConf4 { get; set; }
    public string? TransferConf5 { get; set; }
    public string? XferConfADtmf { get; set; }
    public string? XferConfANumber { get; set; }
    public string? XferConfBDtmf { get; set; }
    public string? XferConfBNumber { get; set; }
    
    // Advanced Features
    public string? ContainerEntry { get; set; }
    public string? ManualDialFilter { get; set; }
    public string? DispoCallUrl { get; set; }
    public string? WebForm1 { get; set; }
    public string? WebForm2 { get; set; }
    public string? WebForm3 { get; set; }
    public int WrapupSeconds { get; set; } = 0;
    public string DialStatuses { get; set; } = "READY,QUEUE";
    public string? LeadFilterId { get; set; }
    public int DropCallTimer { get; set; } = 8;
    public string? SafeHarborMessage { get; set; }
    public decimal MaxDropPercentage { get; set; } = 3.0m;
    
    // Campaign Settings
    public bool IsActive { get; set; } = false;
    public int Priority { get; set; } = 5;
    public string? LocationRestrictions { get; set; }
    public bool EnableCallRecording { get; set; } = false;
    public string? CustomFields { get; set; }
    
    // Lead Recycling
    public string? RecyclingRules { get; set; }
    public bool EnableAutoRecycling { get; set; } = false;
    public int MaxRecycleCount { get; set; } = 3;
}

public class CampaignDto
{
    public int Id { get; set; }
    
    // Core Campaign Properties
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DialMethod { get; set; } = string.Empty;
    public decimal DialingRatio { get; set; }
    public bool ApplyRatioToIdleAgentsOnly { get; set; }
    public decimal AdaptiveMaxLevel { get; set; }
    public int DialTimeout { get; set; }
    public int MaxConcurrentCalls { get; set; }
    
    // Dial Prefixes and Extensions
    public string? DialPrefix { get; set; }
    public string? ManualDialPrefix { get; set; }
    public string? ThreeWayDialPrefix { get; set; }
    public string? ParkExtension { get; set; }
    public string? ParkFileName { get; set; }
    public string? VoicemailExtension { get; set; }
    public string? AnsweringMachineExtension { get; set; }
    
    // Agent Selection and Routing
    public bool AvailableOnlyRatioTally { get; set; }
    public string AvailableOnlyTallyThreshold { get; set; } = string.Empty;
    public string NextAgentCall { get; set; } = string.Empty;
    
    // Call Handling and Features
    public bool AlternateNumberDialing { get; set; }
    public bool ScheduledCallbacks { get; set; }
    public bool AllowInbound { get; set; }
    public bool ForceResetHopper { get; set; }
    public bool HopperDuplicateCheck { get; set; }
    public string GetCallLaunch { get; set; } = string.Empty;
    
    // Time Restrictions
    public string TimeZone { get; set; } = string.Empty;
    public string CallStartTime { get; set; } = string.Empty;
    public string CallEndTime { get; set; } = string.Empty;
    public string AllowedDaysOfWeek { get; set; } = string.Empty;
    public bool RespectLeadTimeZone { get; set; }
    
    // Call Attempt Configuration
    public int MinCallInterval { get; set; }
    public int MaxCallAttempts { get; set; }
    public int CallAttemptDelay { get; set; }
    
    // Answering Machine Detection
    public bool EnableAnsweringMachineDetection { get; set; }
    public string AnsweringMachineAction { get; set; } = string.Empty;
    public string? AnsweringMachineMessage { get; set; }
    
    // Lead Processing
    public string LeadOrder { get; set; } = string.Empty;
    public bool RandomizeLeadOrder { get; set; }
    public string? LeadOrderSecondary { get; set; }
    public int HopperLevel { get; set; }
    
    // Transfer Configuration
    public string? OutboundCallerId { get; set; }
    public string? TransferConf1 { get; set; }
    public string? TransferConf2 { get; set; }
    public string? TransferConf3 { get; set; }
    public string? TransferConf4 { get; set; }
    public string? TransferConf5 { get; set; }
    public string? XferConfADtmf { get; set; }
    public string? XferConfANumber { get; set; }
    public string? XferConfBDtmf { get; set; }
    public string? XferConfBNumber { get; set; }
    
    // Advanced Features
    public string? ContainerEntry { get; set; }
    public string? ManualDialFilter { get; set; }
    public string? DispoCallUrl { get; set; }
    public string? WebForm1 { get; set; }
    public string? WebForm2 { get; set; }
    public string? WebForm3 { get; set; }
    public int WrapupSeconds { get; set; }
    public string DialStatuses { get; set; } = string.Empty;
    public string? LeadFilterId { get; set; }
    public int DropCallTimer { get; set; }
    public string? SafeHarborMessage { get; set; }
    public decimal MaxDropPercentage { get; set; }
    
    // Campaign Settings
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public string? LocationRestrictions { get; set; }
    public bool EnableCallRecording { get; set; }
    public string? CustomFields { get; set; }
    
    // Lead Recycling
    public string? RecyclingRules { get; set; }
    public bool EnableAutoRecycling { get; set; }
    public int MaxRecycleCount { get; set; }
    
    // Statistics and Performance
    public DateTime? StatsLastUpdated { get; set; }
    public int TotalDialedToday { get; set; }
    public int TotalContactsToday { get; set; }
    public decimal CurrentDropRate { get; set; }
    
    // Audit Fields
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}

public class CampaignSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DialMethod { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public decimal DialingRatio { get; set; }
    public int Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Performance Metrics
    public int TotalDialedToday { get; set; }
    public int TotalContactsToday { get; set; }
    public decimal CurrentDropRate { get; set; }
    public DateTime? StatsLastUpdated { get; set; }
    
    // Associated Lists Count
    public int TotalLists { get; set; }
    public int ActiveLists { get; set; }
    public int TotalLeads { get; set; }
    public int CalledLeads { get; set; }
    public int ContactedLeads { get; set; }
}
