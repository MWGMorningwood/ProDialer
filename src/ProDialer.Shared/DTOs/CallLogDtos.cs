namespace ProDialer.Shared.DTOs;

// Call Log DTOs (enhanced for VICIdial compatibility)
public class CallLogDto
{
    public int Id { get; set; }
    public int CampaignId { get; set; }
    public string? CampaignName { get; set; }
    public int LeadId { get; set; }
    public string? LeadName { get; set; }
    public string? CallId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string CallDirection { get; set; } = "Outbound";
    public string CallStatus { get; set; } = "Initiated";
    public string? CallProgress { get; set; }
    public string? AmdResult { get; set; }
    public string? CallOutcome { get; set; }
    public string? HangupCause { get; set; }
    public string? Disposition { get; set; }
    public int? DispositionCodeId { get; set; }
    public string? DispositionCodeName { get; set; }
    public string? Notes { get; set; }
    public string? RecordingUrl { get; set; }
    public string? AgentId { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? AnsweredAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int DurationSeconds { get; set; }
    public int RingDurationSeconds { get; set; }
    public int TalkDurationSeconds { get; set; }
    public int WrapupSeconds { get; set; }
    public decimal? Cost { get; set; }
    public string? CostCurrency { get; set; }
    public bool AnsweringMachineDetected { get; set; }
    public bool WasTransferred { get; set; }
    public string? TransferredToAgent { get; set; }
    public bool WasRecorded { get; set; }
    public bool WasMonitored { get; set; }
    public string? TechnicalDetails { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Tags { get; set; }
    public string? CustomFields { get; set; }
    public string? TimeZone { get; set; }
    public int LeadAttemptNumber { get; set; }
    public string? ComplianceFlags { get; set; }
    public string? ThreeWayParticipants { get; set; }
    public string? TransferDetails { get; set; }
    public bool TranscriptionEnabled { get; set; }
    public string? TranscriptionText { get; set; }
    public double? TranscriptionConfidence { get; set; }
    public string? TranscriptionLanguage { get; set; }
    public string? TranscriptionStatus { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CallLogSummaryDto
{
    public int Id { get; set; }
    public int CampaignId { get; set; }
    public string? CampaignName { get; set; }
    public int LeadId { get; set; }
    public string? LeadName { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string CallDirection { get; set; } = "Outbound";
    public string CallStatus { get; set; } = "Initiated";
    public string? CallOutcome { get; set; }
    public string? Disposition { get; set; }
    public string? AgentId { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int DurationSeconds { get; set; }
    public int TalkDurationSeconds { get; set; }
}

public class CallStatisticsDto
{
    public int TotalCalls { get; set; }
    public int CompletedCalls { get; set; }
    public int AnsweredCalls { get; set; }
    public int ConnectedCalls { get; set; }
    public int VoicemailCalls { get; set; }
    public int BusyCalls { get; set; }
    public int NoAnswerCalls { get; set; }
    public int DisconnectedCalls { get; set; }
    public int ErrorCalls { get; set; }
    public decimal AnswerRate { get; set; }
    public decimal ContactRate { get; set; }
    public int AverageTalkTime { get; set; }
    public int AverageRingTime { get; set; }
    public decimal TotalCost { get; set; }
    public string? CostCurrency { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}

public class CallReportDto
{
    public int CampaignId { get; set; }
    public string? CampaignName { get; set; }
    public int? ListId { get; set; }
    public string? ListName { get; set; }
    public string? AgentId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<CallLogSummaryDto> Calls { get; set; } = new();
    public CallStatisticsDto Statistics { get; set; } = new();
}
