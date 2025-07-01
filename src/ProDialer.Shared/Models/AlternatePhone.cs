using System.ComponentModel.DataAnnotations;

namespace ProDialer.Shared.Models;

/// <summary>
/// VICIdial-style unlimited alternate phone numbers per lead
/// </summary>
public class AlternatePhone
{
    public int Id { get; set; }
    
    /// <summary>
    /// Lead this alternate phone belongs to
    /// </summary>
    public int LeadId { get; set; }
    
    /// <summary>
    /// Phone number
    /// </summary>
    [Required]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Phone country code
    /// </summary>
    [StringLength(4)]
    public string PhoneCode { get; set; } = "1";
    
    /// <summary>
    /// Phone type: HOME, WORK, MOBILE, FAX, OTHER
    /// </summary>
    [StringLength(20)]
    public string PhoneType { get; set; } = "OTHER";
    
    /// <summary>
    /// Priority order for dialing (1 = highest priority)
    /// </summary>
    [Range(1, 99)]
    public int Priority { get; set; } = 1;
    
    /// <summary>
    /// Current status: ACTIVE, INVALID, DNC, DISCONNECTED
    /// </summary>
    [StringLength(20)]
    public string Status { get; set; } = "ACTIVE";
    
    /// <summary>
    /// Number of times this phone has been called
    /// </summary>
    public int CallAttempts { get; set; } = 0;
    
    /// <summary>
    /// Date of last call attempt
    /// </summary>
    public DateTime? LastCalledAt { get; set; }
    
    /// <summary>
    /// Last call outcome: NO_ANSWER, BUSY, ANSWERED, DISCONNECTED, etc.
    /// </summary>
    [StringLength(50)]
    public string? LastCallOutcome { get; set; }
    
    /// <summary>
    /// Whether this number has been validated
    /// </summary>
    public bool IsValidated { get; set; } = false;
    
    /// <summary>
    /// Validation result: VALID, INVALID, UNKNOWN
    /// </summary>
    [StringLength(20)]
    public string ValidationResult { get; set; } = "UNKNOWN";
    
    /// <summary>
    /// Phone carrier information
    /// </summary>
    [StringLength(100)]
    public string? Carrier { get; set; }
    
    /// <summary>
    /// Line type: MOBILE, LANDLINE, VOIP
    /// </summary>
    [StringLength(20)]
    public string? LineType { get; set; }
    
    /// <summary>
    /// Time zone for this number (if different from lead's timezone)
    /// </summary>
    [StringLength(100)]
    public string? TimeZone { get; set; }
    
    /// <summary>
    /// Best time to call this number (JSON): {"start": "09:00", "end": "17:00", "days": ["Monday", "Tuesday"]}
    /// </summary>
    public string? BestCallTime { get; set; }
    
    /// <summary>
    /// Notes about this phone number
    /// </summary>
    [StringLength(500)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Whether this number is active for dialing
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual Lead Lead { get; set; } = null!;
    public virtual ICollection<CallLog> CallLogs { get; set; } = new List<CallLog>();
}
