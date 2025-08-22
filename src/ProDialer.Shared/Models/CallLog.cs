using System.ComponentModel.DataAnnotations;

namespace ProDialer.Shared.Models;

public class CallLog
{
    public int Id { get; set; }
    
    /// <summary>
    /// The campaign this call was made for
    /// </summary>
    public int CampaignId { get; set; }
    
    /// <summary>
    /// The lead that was called
    /// </summary>
    public int LeadId { get; set; }
    
    /// <summary>
    /// The agent who made or received the call
    /// </summary>
    [StringLength(100)]
    public string? AgentId { get; set; }
    
    /// <summary>
    /// Phone number that was dialed
    /// </summary>
    [Required]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Unique identifier for the call from the communication service
    /// </summary>
    [StringLength(200)]
    public string? CallId { get; set; }
    
    /// <summary>
    /// Direction of the call: Outbound, Inbound
    /// </summary>
    [StringLength(20)]
    public string CallDirection { get; set; } = "Outbound";
    
    /// <summary>
    /// Current status of the call: Initiated, Ringing, Connected, Ended, Failed
    /// </summary>
    [StringLength(20)]
    public string CallStatus { get; set; } = "Initiated";
    
    /// <summary>
    /// Call progress states: DIALING, RINGING, BUSY, NO_ANSWER, ANSWERED, HANGUP, FAILED
    /// </summary>
    [StringLength(20)]
    public string? CallProgress { get; set; }
    
    /// <summary>
    /// Answering machine detection result: HUMAN, MACHINE, UNKNOWN
    /// </summary>
    [StringLength(20)]
    public string? AmdResult { get; set; }
    
    /// <summary>
    /// Detailed call outcome: ANSWERED_HUMAN, ANSWERED_MACHINE, BUSY, NO_ANSWER, FAILED, DISCONNECTED
    /// </summary>
    [StringLength(50)]
    public string? CallOutcome { get; set; }
    
    /// <summary>
    /// Hangup cause: NORMAL, BUSY, NO_ANSWER, FAILED, AGENT_HANGUP, CUSTOMER_HANGUP
    /// </summary>
    [StringLength(50)]
    public string? HangupCause { get; set; }
    
    /// <summary>
    /// SIP response code (if applicable)
    /// </summary>
    public int? SipResponseCode { get; set; }
    
    /// <summary>
    /// When the call was initiated
    /// </summary>
    public DateTime StartedAt { get; set; }
    
    /// <summary>
    /// When the call was answered (if applicable)
    /// </summary>
    public DateTime? AnsweredAt { get; set; }
    
    /// <summary>
    /// When the call ended
    /// </summary>
    public DateTime? EndedAt { get; set; }
    
    /// <summary>
    /// Total duration of the call in seconds
    /// </summary>
    public int DurationSeconds { get; set; } = 0;
    
    /// <summary>
    /// Duration the call was ringing before being answered or ended
    /// </summary>
    public int RingDurationSeconds { get; set; } = 0;
    
    /// <summary>
    /// Duration of the actual conversation (after answer, before hangup)
    /// </summary>
    public int TalkDurationSeconds { get; set; } = 0;
    
    /// <summary>
    /// Cost of the call (if available from the communication service)
    /// </summary>
    public decimal? Cost { get; set; }
    
    /// <summary>
    /// Currency for the call cost
    /// </summary>
    [StringLength(3)]
    public string? CostCurrency { get; set; }
    
    /// <summary>
    /// Disposition/result set by the agent: Interested, NotInterested, Callback, Sale, DoNotCall, etc.
    /// </summary>
    [StringLength(50)]
    public string? Disposition { get; set; }
    
    /// <summary>
    /// Notes from the agent about the call
    /// </summary>
    [StringLength(2000)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Whether answering machine was detected
    /// </summary>
    public bool AnsweringMachineDetected { get; set; } = false;
    
    /// <summary>
    /// Confidence level of answering machine detection (0-100)
    /// </summary>
    [Range(0, 100)]
    public int? AnsweringMachineConfidence { get; set; }
    
    /// <summary>
    /// Whether the call was transferred to another agent
    /// </summary>
    public bool WasTransferred { get; set; } = false;
    
    /// <summary>
    /// Agent the call was transferred to
    /// </summary>
    [StringLength(100)]
    public string? TransferredToAgent { get; set; }
    
    /// <summary>
    /// Whether the call was recorded
    /// </summary>
    public bool WasRecorded { get; set; } = false;
    
    /// <summary>
    /// URL or path to the call recording
    /// </summary>
    [StringLength(500)]
    public string? RecordingUrl { get; set; }
    
    /// <summary>
    /// Quality score for the call (if available)
    /// </summary>
    [Range(1, 5)]
    public int? QualityScore { get; set; }
    
    /// <summary>
    /// Technical details about the call connection
    /// </summary>
    [StringLength(1000)]
    public string? TechnicalDetails { get; set; }
    
    /// <summary>
    /// Error message if the call failed
    /// </summary>
    [StringLength(500)]
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Tags for categorizing and analyzing calls
    /// </summary>
    [StringLength(500)]
    public string? Tags { get; set; }
    
    /// <summary>
    /// Custom fields for call-specific data (JSON format)
    /// </summary>
    public string? CustomFields { get; set; }
    
    /// <summary>
    /// Time zone where the call was made
    /// </summary>
    [StringLength(100)]
    public string? TimeZone { get; set; }
    
    /// <summary>
    /// Local time when the call was made (for reporting purposes)
    /// </summary>
    public DateTime? LocalTime { get; set; }
    
    /// <summary>
    /// Lead attempt count at time of this call
    /// </summary>
    public int LeadAttemptNumber { get; set; } = 1;
    
    /// <summary>
    /// Disposition code ID if call was dispositioned
    /// </summary>
    public int? DispositionCodeId { get; set; }
    
    /// <summary>
    /// Agent wrapup time in seconds
    /// </summary>
    public int WrapupSeconds { get; set; } = 0;
    
    /// <summary>
    /// Compliance flags (JSON): {"timezone_valid": true, "dnc_checked": true}
    /// </summary>
    public string? ComplianceFlags { get; set; }
    
    /// <summary>
    /// Whether this call was monitored
    /// </summary>
    public bool WasMonitored { get; set; } = false;
    
    /// <summary>
    /// Three-way call participants (JSON array)
    /// </summary>
    public string? ThreeWayParticipants { get; set; }
    
    /// <summary>
    /// Transfer details if call was transferred (JSON)
    /// </summary>
    public string? TransferDetails { get; set; }
    
    /// <summary>
    /// Whether live transcription was enabled for this call
    /// </summary>
    public bool TranscriptionEnabled { get; set; } = false;
    
    /// <summary>
    /// Full transcription text of the call
    /// </summary>
    public string? TranscriptionText { get; set; }
    
    /// <summary>
    /// Transcription confidence score (0-1)
    /// </summary>
    [Range(0, 1)]
    public double? TranscriptionConfidence { get; set; }
    
    /// <summary>
    /// Language detected in the transcription
    /// </summary>
    [StringLength(10)]
    public string? TranscriptionLanguage { get; set; }
    
    /// <summary>
    /// Status of transcription processing
    /// </summary>
    [StringLength(20)]
    public string? TranscriptionStatus { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Campaign Campaign { get; set; } = null!;
    public virtual Lead Lead { get; set; } = null!;
    public virtual DispositionCode? DispositionCode { get; set; }
}
