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
    
    [StringLength(20)]
    public string? SecondaryPhone { get; set; }
    
    [StringLength(20)]
    public string? MobilePhone { get; set; }
    
    [StringLength(20)]
    public string? WorkPhone { get; set; }
    
    [StringLength(20)]
    public string? HomePhone { get; set; }
    
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
    /// Current status: New, InProgress, Contacted, NotInterested, DoNotCall, Callback, etc.
    /// </summary>
    [StringLength(50)]
    public string Status { get; set; } = "New";
    
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
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual List List { get; set; } = null!;
    public virtual ICollection<CallLog> CallLogs { get; set; } = new List<CallLog>();
}
