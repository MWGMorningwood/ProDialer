namespace ProDialer.Shared.DTOs;

public class CreateAgentDto
{
    public string UserId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public int MaxConcurrentCalls { get; set; } = 1;
    public int SkillLevel { get; set; } = 5;
    public string? QualifiedCampaigns { get; set; }
    public string? Languages { get; set; }
    public string TimeZone { get; set; } = "UTC";
    public string? Role { get; set; }
    public string? Supervisor { get; set; }
    public string? Extension { get; set; }
    public string? CommunicationEndpoint { get; set; }
    public string? Tags { get; set; }
    public string? CustomFields { get; set; }
}

public class UpdateAgentDto : CreateAgentDto
{
    public int Id { get; set; }
    public string Status { get; set; } = "LoggedOut";
    public bool IsLoggedIn { get; set; } = false;
    public bool IsOnCall { get; set; } = false;
    public int ActiveCalls { get; set; } = 0;
    public DateTime? CurrentSessionStartedAt { get; set; }
    public DateTime? LastLoggedOutAt { get; set; }
    public int TodayLoggedInMinutes { get; set; } = 0;
    public int TodayCallsHandled { get; set; } = 0;
    public int TodayTalkTimeMinutes { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}

public class AgentSummaryDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Status { get; set; } = "LoggedOut";
    public bool IsLoggedIn { get; set; } = false;
    public bool IsOnCall { get; set; } = false;
    public int ActiveCalls { get; set; } = 0;
    public int MaxConcurrentCalls { get; set; } = 1;
    public int SkillLevel { get; set; } = 5;
    public string? QualifiedCampaigns { get; set; }
    public string? Languages { get; set; }
    public string TimeZone { get; set; } = "UTC";
    public string? Role { get; set; }
    public string? Supervisor { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Extension { get; set; }
    public DateTime? CurrentSessionStartedAt { get; set; }
    public DateTime? LastLoggedOutAt { get; set; }
    public int TodayLoggedInMinutes { get; set; } = 0;
    public int TodayCallsHandled { get; set; } = 0;
    public int TodayTalkTimeMinutes { get; set; } = 0;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Performance metrics (calculated)
    public int TotalCalls { get; set; }
    public int ConnectedCalls { get; set; }
    public decimal AverageCallDuration { get; set; }
    public decimal ConversionRate { get; set; }
}

public class AgentDetailDto : AgentSummaryDto
{
    // Additional agent fields not in summary
    public string? CommunicationEndpoint { get; set; }
    public string? Tags { get; set; }
    public string? CustomFields { get; set; }
    
    // Extended performance metrics
    public int TotalCallsToday { get; set; }
    public int ConnectedCallsToday { get; set; }
    public decimal TotalTalkTimeToday { get; set; }
    public decimal AverageCallDurationToday { get; set; }
    public int LeadsContactedToday { get; set; }
    public int AppointmentsSetToday { get; set; }
}

public class AgentStatusDto
{
    public int AgentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? StatusReason { get; set; }
    public DateTime StatusUpdatedAt { get; set; }
    public bool IsOnCall { get; set; }
    public int? CurrentCallId { get; set; }
    public string? CurrentCallStatus { get; set; }
    public DateTime? CallStartedAt { get; set; }
}

public class UpdateAgentStatusDto
{
    public string Status { get; set; } = string.Empty;
    public string? StatusReason { get; set; }
}
