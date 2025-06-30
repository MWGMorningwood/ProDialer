using System.ComponentModel.DataAnnotations;

namespace ProDialer.Shared.Models;

public class Campaign
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    /// <summary>
    /// How many outbound calls per agent are allowed
    /// </summary>
    [Range(1, 10)]
    public int DialingRatio { get; set; } = 1;
    
    /// <summary>
    /// Whether dialing ratio applies to all logged-in agents or only idle ones
    /// </summary>
    public bool ApplyRatioToIdleAgentsOnly { get; set; } = true;
    
    /// <summary>
    /// Maximum number of concurrent calls for this campaign
    /// </summary>
    [Range(1, 1000)]
    public int MaxConcurrentCalls { get; set; } = 50;
    
    /// <summary>
    /// Time zone for the campaign (affects local time restrictions)
    /// </summary>
    [StringLength(100)]
    public string TimeZone { get; set; } = "UTC";
    
    /// <summary>
    /// Earliest time to start calling (24-hour format, e.g., "08:00")
    /// </summary>
    [StringLength(5)]
    public string CallStartTime { get; set; } = "08:00";
    
    /// <summary>
    /// Latest time to stop calling (24-hour format, e.g., "20:00")
    /// </summary>
    [StringLength(5)]
    public string CallEndTime { get; set; } = "20:00";
    
    /// <summary>
    /// Days of the week when calling is allowed (comma-separated: Monday,Tuesday,etc.)
    /// </summary>
    [StringLength(200)]
    public string AllowedDaysOfWeek { get; set; } = "Monday,Tuesday,Wednesday,Thursday,Friday";
    
    /// <summary>
    /// Whether to respect local time zones of leads
    /// </summary>
    public bool RespectLeadTimeZone { get; set; } = true;
    
    /// <summary>
    /// Minimum time between calls to the same lead (in minutes)
    /// </summary>
    [Range(0, 10080)] // 0 to 7 days
    public int MinCallInterval { get; set; } = 60;
    
    /// <summary>
    /// Maximum number of call attempts per lead
    /// </summary>
    [Range(1, 20)]
    public int MaxCallAttempts { get; set; } = 3;
    
    /// <summary>
    /// Delay between call attempts (in minutes)
    /// </summary>
    [Range(1, 1440)] // 1 minute to 24 hours
    public int CallAttemptDelay { get; set; } = 15;
    
    /// <summary>
    /// Whether to enable answering machine detection
    /// </summary>
    public bool EnableAnsweringMachineDetection { get; set; } = true;
    
    /// <summary>
    /// What to do when answering machine is detected: Hangup, LeaveMessage, TransferToAgent
    /// </summary>
    [StringLength(50)]
    public string AnsweringMachineAction { get; set; } = "Hangup";
    
    /// <summary>
    /// Script or message to play when answering machine is detected
    /// </summary>
    [StringLength(2000)]
    public string? AnsweringMachineMessage { get; set; }
    
    /// <summary>
    /// Whether the campaign is currently active
    /// </summary>
    public bool IsActive { get; set; } = false;
    
    /// <summary>
    /// Priority level for this campaign (higher number = higher priority)
    /// </summary>
    [Range(1, 10)]
    public int Priority { get; set; } = 5;
    
    /// <summary>
    /// Location/region restrictions (comma-separated country codes or regions)
    /// </summary>
    [StringLength(500)]
    public string? LocationRestrictions { get; set; }
    
    /// <summary>
    /// Whether to enable call recording
    /// </summary>
    public bool EnableCallRecording { get; set; } = false;
    
    /// <summary>
    /// Custom fields for campaign-specific data (JSON format)
    /// </summary>
    public string? CustomFields { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual ICollection<CampaignList> CampaignLists { get; set; } = new List<CampaignList>();
    public virtual ICollection<CallLog> CallLogs { get; set; } = new List<CallLog>();
}
