namespace ProDialer.Shared.DTOs;

/// <summary>
/// DTO for dialing engine statistics
/// </summary>
public class DialingStatisticsDto
{
    public int ActiveCampaigns { get; set; }
    public int AvailableAgents { get; set; }
    public int AgentsOnCall { get; set; }
    public int CallsToday { get; set; }
    public int AnsweredCallsToday { get; set; }
    public int CallsInProgress { get; set; }
    public decimal AnswerRateToday { get; set; }
    public int LeadsReadyForCalling { get; set; }
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// DTO for campaign control operations
/// </summary>
public class CampaignControlResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// DTO for active calls in the system
/// </summary>
public class ActiveCallDto
{
    public string CallId { get; set; } = string.Empty;
    public int CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public int LeadId { get; set; }
    public string LeadName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string AgentUsername { get; set; } = string.Empty;
    public string CallStatus { get; set; } = string.Empty;
    public DateTime CallStartTime { get; set; }
    public int CallDurationSeconds { get; set; }
}

/// <summary>
/// DTO for call events from Azure Communication Services
/// </summary>
public class CallEventDto
{
    public string CallId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string CallState { get; set; } = string.Empty;
    public DateTime EventTime { get; set; }
    public string? FromPhoneNumber { get; set; }
    public string? ToPhoneNumber { get; set; }
    public string? HangupReason { get; set; }
    public int? CallDuration { get; set; }
}
