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
    /// Dialing method: Manual, Ratio, Predictive, Preview, Adaptive Average, Adaptive Hard Limit, Adaptive Tapered
    /// </summary>
    [StringLength(50)]
    public string DialMethod { get; set; } = "Manual";
    
    /// <summary>
    /// How many outbound calls per agent are allowed (0.1 to 18.0 for precise control)
    /// </summary>
    [Range(0.1, 18.0)]
    public decimal DialingRatio { get; set; } = 1.0m;
    
    /// <summary>
    /// Whether dialing ratio applies to all logged-in agents or only idle ones
    /// </summary>
    public bool ApplyRatioToIdleAgentsOnly { get; set; } = true;
    
    /// <summary>
    /// Maximum auto-dial level for adaptive dialing
    /// </summary>
    [Range(0.1, 20.0)]
    public decimal AdaptiveMaxLevel { get; set; } = 2.0m;
    
    /// <summary>
    /// Dial timeout in seconds before abandoning call
    /// </summary>
    [Range(1, 120)]
    public int DialTimeout { get; set; } = 30;
    
    /// <summary>
    /// Maximum number of concurrent calls for this campaign
    /// </summary>
    [Range(1, 1000)]
    public int MaxConcurrentCalls { get; set; } = 50;
    
    /// <summary>
    /// Dial prefix for outbound calls (e.g., "9" for external line)
    /// </summary>
    [StringLength(10)]
    public string? DialPrefix { get; set; }
    
    /// <summary>
    /// Manual dial prefix for agent manual dialing
    /// </summary>
    [StringLength(10)]
    public string? ManualDialPrefix { get; set; }
    
    /// <summary>
    /// Three-way dial prefix for conference calls
    /// </summary>
    [StringLength(10)]
    public string? ThreeWayDialPrefix { get; set; }
    
    /// <summary>
    /// Whether to count only available agents for ratio calculations
    /// </summary>
    public bool AvailableOnlyRatioTally { get; set; } = true;
    
    /// <summary>
    /// Threshold setting for available-only tally (DISABLE, WAITING_ONLY, etc.)
    /// </summary>
    [StringLength(50)]
    public string AvailableOnlyTallyThreshold { get; set; } = "DISABLE";
    
    /// <summary>
    /// Method for selecting next agent: longest_wait_time, random, ring_all, etc.
    /// </summary>
    [StringLength(50)]
    public string NextAgentCall { get; set; } = "longest_wait_time";
    
    /// <summary>
    /// Extension for call parking
    /// </summary>
    [StringLength(20)]
    public string? ParkExtension { get; set; }
    
    /// <summary>
    /// File to play while call is parked
    /// </summary>
    [StringLength(200)]
    public string? ParkFileName { get; set; }
    
    /// <summary>
    /// Voicemail extension for dropped calls
    /// </summary>
    [StringLength(20)]
    public string? VoicemailExtension { get; set; }
    
    /// <summary>
    /// Enable alternate number dialing for leads
    /// </summary>
    public bool AlternateNumberDialing { get; set; } = true;
    
    /// <summary>
    /// Enable scheduled callback functionality
    /// </summary>
    public bool ScheduledCallbacks { get; set; } = true;
    
    /// <summary>
    /// Allow inbound calls to this campaign
    /// </summary>
    public bool AllowInbound { get; set; } = false;
    
    /// <summary>
    /// Force hopper reset on campaign activation
    /// </summary>
    public bool ForceResetHopper { get; set; } = false;
    
    /// <summary>
    /// Check for duplicates when adding leads to hopper
    /// </summary>
    public bool HopperDuplicateCheck { get; set; } = true;
    
    /// <summary>
    /// Method for launching calls: NONE, SCRIPT, WEBFORM, etc.
    /// </summary>
    [StringLength(50)]
    public string GetCallLaunch { get; set; } = "NONE";
    
    /// <summary>
    /// Extension for answering machine detection messages
    /// </summary>
    [StringLength(20)]
    public string? AnsweringMachineExtension { get; set; }
    
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
    /// Lead order strategy: UP, DOWN, RANDOM, OLDEST_FIRST, NEWEST_FIRST, etc.
    /// </summary>
    [StringLength(50)]
    public string LeadOrder { get; set; } = "UP";
    
    /// <summary>
    /// Whether to randomize lead order within the strategy
    /// </summary>
    public bool RandomizeLeadOrder { get; set; } = false;
    
    /// <summary>
    /// Secondary ordering method for lead selection
    /// </summary>
    [StringLength(50)]
    public string? LeadOrderSecondary { get; set; }
    
    /// <summary>
    /// Hopper level - number of leads to keep in queue per campaign
    /// </summary>
    [Range(1, 2000)]
    public int HopperLevel { get; set; } = 100;
    
    /// <summary>
    /// Caller ID number for outbound calls
    /// </summary>
    [StringLength(20)]
    public string? OutboundCallerId { get; set; }
    
    /// <summary>
    /// Transfer conference number 1
    /// </summary>
    [StringLength(50)]
    public string? TransferConf1 { get; set; }
    
    /// <summary>
    /// Transfer conference number 2
    /// </summary>
    [StringLength(50)]
    public string? TransferConf2 { get; set; }
    
    /// <summary>
    /// Transfer conference number 3
    /// </summary>
    [StringLength(50)]
    public string? TransferConf3 { get; set; }
    
    /// <summary>
    /// Transfer conference number 4
    /// </summary>
    [StringLength(50)]
    public string? TransferConf4 { get; set; }
    
    /// <summary>
    /// Transfer conference number 5
    /// </summary>
    [StringLength(50)]
    public string? TransferConf5 { get; set; }
    
    /// <summary>
    /// Transfer A DTMF sequence
    /// </summary>
    [StringLength(50)]
    public string? XferConfADtmf { get; set; }
    
    /// <summary>
    /// Transfer A number
    /// </summary>
    [StringLength(50)]
    public string? XferConfANumber { get; set; }
    
    /// <summary>
    /// Transfer B DTMF sequence  
    /// </summary>
    [StringLength(50)]
    public string? XferConfBDtmf { get; set; }
    
    /// <summary>
    /// Transfer B number
    /// </summary>
    [StringLength(50)]
    public string? XferConfBNumber { get; set; }
    
    /// <summary>
    /// Container entry for lead processing
    /// </summary>
    [StringLength(50)]
    public string? ContainerEntry { get; set; }
    
    /// <summary>
    /// Manual dial filter for agent dialing restrictions
    /// </summary>
    [StringLength(200)]
    public string? ManualDialFilter { get; set; }
    
    /// <summary>
    /// Disposition call URL for integration
    /// </summary>
    [StringLength(2000)]
    public string? DispoCallUrl { get; set; }
    
    /// <summary>
    /// Web form URL 1 for agent interface
    /// </summary>
    [StringLength(2000)]
    public string? WebForm1 { get; set; }
    
    /// <summary>
    /// Web form URL 2 for agent interface
    /// </summary>
    [StringLength(2000)]
    public string? WebForm2 { get; set; }
    
    /// <summary>
    /// Web form URL 3 for agent interface
    /// </summary>
    [StringLength(2000)]
    public string? WebForm3 { get; set; }
    
    /// <summary>
    /// Default agent wrapup time in seconds
    /// </summary>
    [Range(0, 9999)]
    public int WrapupSeconds { get; set; } = 15;
    
    /// <summary>
    /// Statuses that should be automatically dialed (space-separated)
    /// </summary>
    [StringLength(200)]
    public string DialStatuses { get; set; } = "NEW";
    
    /// <summary>
    /// Lead filter ID for filtering leads
    /// </summary>
    [StringLength(50)]
    public string? LeadFilterId { get; set; }
    
    /// <summary>
    /// Drop call timer for compliance (seconds)
    /// </summary>
    [Range(0, 120)]
    public int DropCallTimer { get; set; } = 0;
    
    /// <summary>
    /// Safe harbor message for dropped calls
    /// </summary>
    [StringLength(2000)]
    public string? SafeHarborMessage { get; set; }
    
    /// <summary>
    /// Maximum drop percentage for compliance
    /// </summary>
    [Range(0, 100)]
    public decimal MaxDropPercentage { get; set; } = 3.0m;
    
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
    
    /// <summary>
    /// Lead recycling rules (JSON format): [{"status": "DROP", "delay_hours": 24}, ...]
    /// </summary>
    public string? RecyclingRules { get; set; }
    
    /// <summary>
    /// Auto-recycling enabled flag
    /// </summary>
    public bool EnableAutoRecycling { get; set; } = false;
    
    /// <summary>
    /// Maximum times a lead can be recycled
    /// </summary>
    [Range(0, 10)]
    public int MaxRecycleCount { get; set; } = 3;
    
    /// <summary>
    /// Campaign statistics update timestamp
    /// </summary>
    public DateTime? StatsLastUpdated { get; set; }
    
    /// <summary>
    /// Total leads dialed today
    /// </summary>
    public int TotalDialedToday { get; set; } = 0;
    
    /// <summary>
    /// Total contacts made today
    /// </summary>
    public int TotalContactsToday { get; set; } = 0;
    
    /// <summary>
    /// Current drop rate percentage
    /// </summary>
    public decimal CurrentDropRate { get; set; } = 0.0m;
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual ICollection<CampaignList> CampaignLists { get; set; } = new List<CampaignList>();
    public virtual ICollection<CallLog> CallLogs { get; set; } = new List<CallLog>();
}
