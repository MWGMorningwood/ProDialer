using System.ComponentModel.DataAnnotations;

namespace ProDialer.Shared.Models;

public class Lead
{
    public int Id { get; set; }
    
    /// <summary>
    /// The list this lead belongs to
    /// </summary>
    public int ListId { get; set; }
    
    // Core contact information
    [StringLength(100)]
    public string? FirstName { get; set; }
    
    [StringLength(100)]
    public string? LastName { get; set; }
    
    [StringLength(200)]
    public string? FullName { get; set; }
    
    [StringLength(200)]
    public string? Company { get; set; }
    
    [StringLength(100)]
    public string? Title { get; set; }
    
    // Phone numbers (primary is the main calling number)
    [Required]
    [StringLength(20)]
    public string PrimaryPhone { get; set; } = string.Empty;
    
    /// <summary>
    /// Phone country code (1 for US/Canada, 44 for UK, etc.)
    /// </summary>
    [StringLength(4)]
    public string PhoneCode { get; set; } = "1";
    
    /// <summary>
    /// Raw phone number storage (digits only, no formatting)
    /// </summary>
    [StringLength(20)]
    public string? PhoneNumberRaw { get; set; }
    
    /// <summary>
    /// Phone number validation status: VALID, INVALID, UNVALIDATED
    /// </summary>
    [StringLength(20)]
    public string PhoneValidationStatus { get; set; } = "UNVALIDATED";
    
    /// <summary>
    /// Phone number carrier information (if available)
    /// </summary>
    [StringLength(100)]
    public string? PhoneCarrier { get; set; }
    
    /// <summary>
    /// Phone number type: MOBILE, LANDLINE, VOIP, etc.
    /// </summary>
    [StringLength(20)]
    public string? PhoneType { get; set; }
    
    [StringLength(20)]
    public string? SecondaryPhone { get; set; }
    
    [StringLength(20)]
    public string? MobilePhone { get; set; }
    
    [StringLength(20)]
    public string? WorkPhone { get; set; }
    
    [StringLength(20)]
    public string? HomePhone { get; set; }
    
    /// <summary>
    /// Additional phone numbers (formatted as: phone_type:number,phone_type:number)
    /// </summary>
    [StringLength(1000)]
    public string? AlternatePhones { get; set; }
    
    /// <summary>
    /// External vendor lead code for integration
    /// </summary>
    [StringLength(20)]
    public string? VendorLeadCode { get; set; }
    
    /// <summary>
    /// Source ID for tracking lead origin
    /// </summary>
    [StringLength(50)]
    public string? SourceId { get; set; }
    
    /// <summary>
    /// Middle initial
    /// </summary>
    [StringLength(1)]
    public string? MiddleInitial { get; set; }
    
    /// <summary>
    /// Address line 3 for international addresses
    /// </summary>
    [StringLength(200)]
    public string? AddressLine3 { get; set; }
    
    /// <summary>
    /// Province for international addresses
    /// </summary>
    [StringLength(100)]
    public string? Province { get; set; }
    
    /// <summary>
    /// Security phrase for verification
    /// </summary>
    [StringLength(100)]
    public string? SecurityPhrase { get; set; }
    
    /// <summary>
    /// Rank for priority ordering (higher = more important)
    /// </summary>
    [Range(0, 99999)]
    public int Rank { get; set; } = 0;
    
    /// <summary>
    /// Lead called count (number of times attempted)
    /// </summary>
    public int CalledCount { get; set; } = 0;
    
    /// <summary>
    /// Lead quality score (calculated or assigned)
    /// </summary>
    [Range(0, 100)]
    public int QualityScore { get; set; } = 50;
    
    /// <summary>
    /// Lead lifecycle stage: NEW, CONTACTED, QUALIFIED, CONVERTED, DEAD
    /// </summary>
    [StringLength(20)]
    public string LifecycleStage { get; set; } = "NEW";
    
    /// <summary>
    /// Lead recycling count (how many times recycled)
    /// </summary>
    public int RecycleCount { get; set; } = 0;
    
    /// <summary>
    /// Last recycle date/time
    /// </summary>
    public DateTime? LastRecycledAt { get; set; }
    
    /// <summary>
    /// Compliance flags (JSON): {"dnc_checked": true, "timezone_valid": true}
    /// </summary>
    public string? ComplianceFlags { get; set; }
    
    /// <summary>
    /// Callback appointment details (JSON): {"date": "2025-01-15T10:00:00Z", "agent": "agent1", "notes": "..."}
    /// </summary>
    public string? CallbackAppointment { get; set; }
    
    /// <summary>
    /// Owner/territory assignment
    /// </summary>
    [StringLength(20)]
    public string? Owner { get; set; }
    
    /// <summary>
    /// Entry list ID for custom fields tracking
    /// </summary>
    public int? EntryListId { get; set; }
    
    /// <summary>
    /// GMT offset for this lead's timezone
    /// </summary>
    [Range(-12, 14)]
    public decimal GmtOffset { get; set; } = 0;
    
    /// <summary>
    /// Current GMT offset accounting for DST
    /// </summary>
    [Range(-12, 14)]
    public decimal GmtOffsetNow { get; set; } = 0;
    
    // Email addresses
    [StringLength(200)]
    [EmailAddress]
    public string? PrimaryEmail { get; set; }
    
    [StringLength(200)]
    [EmailAddress]
    public string? SecondaryEmail { get; set; }
    
    // Address information
    [StringLength(200)]
    public string? AddressLine1 { get; set; }
    
    [StringLength(200)]
    public string? AddressLine2 { get; set; }
    
    [StringLength(100)]
    public string? City { get; set; }
    
    [StringLength(100)]
    public string? State { get; set; }
    
    [StringLength(20)]
    public string? PostalCode { get; set; }
    
    [StringLength(100)]
    public string? Country { get; set; }
    
    /// <summary>
    /// Time zone for this lead (used for local time calling restrictions)
    /// </summary>
    [StringLength(100)]
    public string? TimeZone { get; set; }
    
    // Call management fields
    /// <summary>
    /// Current status: NEW, CALLED, BUSY, NA, A, DROP, etc. (VICIdial style codes)
    /// </summary>
    [StringLength(6)]
    public string Status { get; set; } = "NEW";
    
    /// <summary>
    /// User/agent who last handled this lead
    /// </summary>
    [StringLength(20)]
    public string? User { get; set; }
    
    /// <summary>
    /// Priority for calling this lead (higher number = higher priority)
    /// </summary>
    [Range(1, 10)]
    public int Priority { get; set; } = 5;
    
    /// <summary>
    /// Number of times this lead has been called
    /// </summary>
    public int CallAttempts { get; set; } = 0;
    
    /// <summary>
    /// Local call time for last call attempt
    /// </summary>
    public DateTime? LastLocalCallTime { get; set; }
    
    /// <summary>
    /// Called since last reset flag
    /// </summary>
    public bool CalledSinceLastReset { get; set; } = false;
    
    /// <summary>
    /// Date/time of the last call attempt
    /// </summary>
    public DateTime? LastCalledAt { get; set; }
    
    /// <summary>
    /// Date/time of the last successful contact
    /// </summary>
    public DateTime? LastContactedAt { get; set; }
    
    /// <summary>
    /// Date/time when this lead should be available for the next call
    /// </summary>
    public DateTime? NextCallAt { get; set; }
    
    /// <summary>
    /// Scheduled callback date/time
    /// </summary>
    public DateTime? ScheduledCallbackAt { get; set; }
    
    /// <summary>
    /// Agent who last handled this lead
    /// </summary>
    [StringLength(100)]
    public string? LastHandlerAgent { get; set; }
    
    /// <summary>
    /// Notes from the last call or interaction
    /// </summary>
    [StringLength(2000)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Outcome of the last call: NoAnswer, Busy, Answered, Voicemail, etc.
    /// </summary>
    [StringLength(50)]
    public string? LastCallOutcome { get; set; }
    
    /// <summary>
    /// Disposition/result: Interested, NotInterested, CallBack, DoNotCall, Sale, etc.
    /// </summary>
    [StringLength(50)]
    public string? Disposition { get; set; }
    
    // Demographics and additional info
    [StringLength(10)]
    public string? Gender { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    
    [StringLength(20)]
    public string? MaritalStatus { get; set; }
    
    [StringLength(100)]
    public string? Income { get; set; }
    
    [StringLength(100)]
    public string? Education { get; set; }
    
    [StringLength(200)]
    public string? Interests { get; set; }
    
    /// <summary>
    /// Source where this lead was acquired
    /// </summary>
    [StringLength(200)]
    public string? Source { get; set; }
    
    /// <summary>
    /// Campaign or medium that generated this lead
    /// </summary>
    [StringLength(200)]
    public string? SourceCampaign { get; set; }
    
    /// <summary>
    /// Cost or value associated with acquiring this lead
    /// </summary>
    public decimal? LeadValue { get; set; }
    
    /// <summary>
    /// Expected or actual revenue from this lead
    /// </summary>
    public decimal? ExpectedRevenue { get; set; }
    
    /// <summary>
    /// Tags for categorizing and filtering leads
    /// </summary>
    [StringLength(500)]
    public string? Tags { get; set; }
    
    /// <summary>
    /// Custom fields specific to this lead (JSON format)
    /// Allows for flexible, customer-specific data storage
    /// </summary>
    public string? CustomFields { get; set; }
    
    /// <summary>
    /// Whether this lead should be excluded from calling (DNC list, opt-out, etc.)
    /// </summary>
    public bool IsExcluded { get; set; } = false;
    
    /// <summary>
    /// Reason for exclusion
    /// </summary>
    [StringLength(200)]
    public string? ExclusionReason { get; set; }
    
    /// <summary>
    /// Date when the lead was excluded
    /// </summary>
    public DateTime? ExcludedAt { get; set; }
    
    /// <summary>
    /// Whether this lead has opted out of communications
    /// </summary>
    public bool HasOptedOut { get; set; } = false;
    
    /// <summary>
    /// Date when the lead opted out
    /// </summary>
    public DateTime? OptedOutAt { get; set; }
    
    /// <summary>
    /// Last modification timestamp (for change tracking)
    /// </summary>
    public DateTime ModifyDate { get; set; }
    
    /// <summary>
    /// Lead conversion tracking: converted value, date, etc. (JSON format)
    /// </summary>
    public string? ConversionTracking { get; set; }
    
    /// <summary>
    /// Agent comments and interaction history (JSON array format)
    /// </summary>
    public string? InteractionHistory { get; set; }
    
    /// <summary>
    /// Lead scoring factors (JSON): {"demographic_score": 25, "behavioral_score": 30}
    /// </summary>
    public string? ScoringFactors { get; set; }
    
    /// <summary>
    /// Marketing attribution data (JSON): {"campaign": "email_2024_q4", "source": "google_ads"}
    /// </summary>
    public string? AttributionData { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual List List { get; set; } = null!;
    public virtual ICollection<CallLog> CallLogs { get; set; } = new List<CallLog>();
}
