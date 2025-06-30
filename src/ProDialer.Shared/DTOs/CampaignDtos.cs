namespace ProDialer.Shared.DTOs;

public class CreateCampaignDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DialingRatio { get; set; } = 1;
    public bool ApplyRatioToIdleAgentsOnly { get; set; } = true;
    public int MaxConcurrentCalls { get; set; } = 50;
    public string? TimeZone { get; set; }
    public string? CallStartTime { get; set; }
    public string? CallEndTime { get; set; }
    public string? AllowedDaysOfWeek { get; set; }
    public bool RespectLeadTimeZone { get; set; } = true;
    public int MinCallInterval { get; set; } = 60;
    public int MaxCallAttempts { get; set; } = 3;
    public int CallAttemptDelay { get; set; } = 15;
    public bool EnableAnsweringMachineDetection { get; set; } = true;
    public string? AnsweringMachineAction { get; set; }
    public string? AnsweringMachineMessage { get; set; }
    public bool IsActive { get; set; } = false;
    public int Priority { get; set; } = 5;
    public string? LocationRestrictions { get; set; }
    public bool EnableCallRecording { get; set; } = false;
    public string? CustomFields { get; set; }
}

public class UpdateCampaignDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DialingRatio { get; set; }
    public bool ApplyRatioToIdleAgentsOnly { get; set; }
    public int MaxConcurrentCalls { get; set; }
    public string? TimeZone { get; set; }
    public string? CallStartTime { get; set; }
    public string? CallEndTime { get; set; }
    public string? AllowedDaysOfWeek { get; set; }
    public bool RespectLeadTimeZone { get; set; }
    public int MinCallInterval { get; set; }
    public int MaxCallAttempts { get; set; }
    public int CallAttemptDelay { get; set; }
    public bool EnableAnsweringMachineDetection { get; set; }
    public string? AnsweringMachineAction { get; set; }
    public string? AnsweringMachineMessage { get; set; }
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public string? LocationRestrictions { get; set; }
    public bool EnableCallRecording { get; set; }
    public string? CustomFields { get; set; }
}

public class CampaignDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DialingRatio { get; set; }
    public bool ApplyRatioToIdleAgentsOnly { get; set; }
    public int MaxConcurrentCalls { get; set; }
    public string TimeZone { get; set; } = string.Empty;
    public string CallStartTime { get; set; } = string.Empty;
    public string CallEndTime { get; set; } = string.Empty;
    public string AllowedDaysOfWeek { get; set; } = string.Empty;
    public bool RespectLeadTimeZone { get; set; }
    public int MinCallInterval { get; set; }
    public int MaxCallAttempts { get; set; }
    public int CallAttemptDelay { get; set; }
    public bool EnableAnsweringMachineDetection { get; set; }
    public string AnsweringMachineAction { get; set; } = string.Empty;
    public string? AnsweringMachineMessage { get; set; }
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public string? LocationRestrictions { get; set; }
    public bool EnableCallRecording { get; set; }
    public string? CustomFields { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CampaignSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int DialingRatio { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int TotalLeads { get; set; }
    public int CalledLeads { get; set; }
    public int ContactedLeads { get; set; }
}
