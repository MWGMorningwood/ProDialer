namespace ProDialer.Shared.DTOs;

public class CreateAgentDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Department { get; set; }
    public string? Team { get; set; }
    public string? Supervisor { get; set; }
    public DateTime? HireDate { get; set; }
    public string? Skills { get; set; }
    public string? Languages { get; set; }
    public bool CanHandleInbound { get; set; } = true;
    public bool CanHandleOutbound { get; set; } = true;
    public decimal? HourlyRate { get; set; }
    public string? Notes { get; set; }
}

public class UpdateAgentDto : CreateAgentDto
{
    public int Id { get; set; }
    public bool IsActive { get; set; } = true;
    public string Status { get; set; } = "Available";
    public DateTime? LastLoginAt { get; set; }
    public DateTime? LastLogoutAt { get; set; }
}

public class AgentSummaryDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Department { get; set; }
    public string? Team { get; set; }
    public bool IsActive { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? LastLoginAt { get; set; }
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
    public string? Supervisor { get; set; }
    public DateTime? HireDate { get; set; }
    public string? Skills { get; set; }
    public string? Languages { get; set; }
    public bool CanHandleInbound { get; set; }
    public bool CanHandleOutbound { get; set; }
    public decimal? HourlyRate { get; set; }
    public string? Notes { get; set; }
    public DateTime? LastLogoutAt { get; set; }
    
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
