using System.ComponentModel.DataAnnotations;

namespace ProDialer.Shared.Models;

public class List
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Priority for this list within campaigns (higher number = higher priority)
    /// </summary>
    [Range(1, 10)]
    public int Priority { get; set; } = 5;
    
    /// <summary>
    /// Strategy for calling leads from this list (Sequential, Random, Priority)
    /// </summary>
    [StringLength(50)]
    public string CallStrategy { get; set; } = "Sequential";
    
    /// <summary>
    /// Maximum number of call attempts for leads in this list (overrides campaign setting if specified)
    /// </summary>
    [Range(0, 20)] // 0 means use campaign setting
    public int? MaxCallAttempts { get; set; }
    
    /// <summary>
    /// Minimum time between calls for leads in this list (overrides campaign setting if specified)
    /// </summary>
    [Range(0, 10080)] // 0 means use campaign setting
    public int? MinCallInterval { get; set; }
    
    /// <summary>
    /// Time zone override for all leads in this list
    /// </summary>
    [StringLength(100)]
    public string? TimeZoneOverride { get; set; }
    
    /// <summary>
    /// Custom calling hours for this list (overrides campaign settings)
    /// </summary>
    [StringLength(5)]
    public string? CallStartTimeOverride { get; set; }
    
    [StringLength(5)]
    public string? CallEndTimeOverride { get; set; }
    
    /// <summary>
    /// Custom allowed days for this list (overrides campaign settings)
    /// </summary>
    [StringLength(200)]
    public string? AllowedDaysOverride { get; set; }
    
    /// <summary>
    /// Whether this list is currently active for calling
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Source of the list data (Upload, API, Manual, etc.)
    /// </summary>
    [StringLength(100)]
    public string Source { get; set; } = "Manual";
    
    /// <summary>
    /// File name or source identifier for imported lists
    /// </summary>
    [StringLength(500)]
    public string? SourceReference { get; set; }
    
    /// <summary>
    /// Total number of leads in this list
    /// </summary>
    public int TotalLeads { get; set; } = 0;
    
    /// <summary>
    /// Number of leads that have been called at least once
    /// </summary>
    public int CalledLeads { get; set; } = 0;
    
    /// <summary>
    /// Number of leads that resulted in successful contacts
    /// </summary>
    public int ContactedLeads { get; set; } = 0;
    
    /// <summary>
    /// Custom fields definition for leads in this list (JSON schema)
    /// </summary>
    public string? CustomFieldsSchema { get; set; }
    
    /// <summary>
    /// Script ID to use for this list
    /// </summary>
    [StringLength(50)]
    public string? ScriptId { get; set; }
    
    /// <summary>
    /// Agent script override for this list
    /// </summary>
    [StringLength(50)]
    public string? AgentScriptOverride { get; set; }
    
    /// <summary>
    /// Campaign caller ID override for this list
    /// </summary>
    [StringLength(20)]
    public string? CampaignCallerIdOverride { get; set; }
    
    /// <summary>
    /// List mixing ratio for multi-list campaigns (0.0-1.0)
    /// </summary>
    [Range(0.0, 1.0)]
    public decimal ListMixRatio { get; set; } = 1.0m;
    
    /// <summary>
    /// Duplicate checking method: NONE, PHONE, PHONE_EMAIL, PHONE_FULL_NAME, etc.
    /// </summary>
    [StringLength(50)]
    public string DuplicateCheckMethod { get; set; } = "PHONE";
    
    /// <summary>
    /// Copy custom fields when handling duplicates
    /// </summary>
    public bool CustomFieldsCopy { get; set; } = false;
    
    /// <summary>
    /// Allow modification of custom fields
    /// </summary>
    public bool CustomFieldsModify { get; set; } = true;
    
    /// <summary>
    /// Reset lead called count on list reset
    /// </summary>
    public bool ResetLeadCalledCount { get; set; } = true;
    
    /// <summary>
    /// Phone number validation settings (JSON): {"validate_nanpa": true, "validate_international": false}
    /// </summary>
    public string? PhoneValidationSettings { get; set; }
    
    /// <summary>
    /// Import/export tracking information
    /// </summary>
    [StringLength(500)]
    public string? ImportExportLog { get; set; }
    
    /// <summary>
    /// List performance metrics (JSON format)
    /// </summary>
    public string? PerformanceMetrics { get; set; }
    
    /// <summary>
    /// Answering machine message for this list
    /// </summary>
    [StringLength(2000)]
    public string? AnsweringMachineMessage { get; set; }
    
    /// <summary>
    /// Drop in-group for calls that can't be handled
    /// </summary>
    [StringLength(50)]
    public string? DropInGroup { get; set; }
    
    /// <summary>
    /// Web form address for this list
    /// </summary>
    [StringLength(2000)]
    public string? WebFormAddress { get; set; }
    
    /// <summary>
    /// Web form address 2 for this list
    /// </summary>
    [StringLength(2000)]
    public string? WebFormAddress2 { get; set; }
    
    /// <summary>
    /// Web form address 3 for this list
    /// </summary>
    [StringLength(2000)]
    public string? WebFormAddress3 { get; set; }
    
    /// <summary>
    /// Reset time for recycling leads (format: 0900-1700-2359)
    /// </summary>
    [StringLength(100)]
    public string? ResetTime { get; set; }
    
    /// <summary>
    /// Timezone detection method: COUNTRY_AND_AREA_CODE, POSTAL_CODE, NANPA_PREFIX, OWNER_TIME_ZONE_CODE
    /// </summary>
    [StringLength(50)]
    public string TimezoneMethod { get; set; } = "COUNTRY_AND_AREA_CODE";
    
    /// <summary>
    /// Local call time ID override
    /// </summary>
    [StringLength(50)]
    public string? LocalCallTime { get; set; }
    
    /// <summary>
    /// Expiration date for this list
    /// </summary>
    public DateTime? ExpirationDate { get; set; }
    
    /// <summary>
    /// Outbound caller ID override for this list
    /// </summary>
    [StringLength(20)]
    public string? OutboundCallerId { get; set; }
    
    /// <summary>
    /// Transfer conference number 1 override
    /// </summary>
    [StringLength(50)]
    public string? TransferConf1 { get; set; }
    
    /// <summary>
    /// Transfer conference number 2 override
    /// </summary>
    [StringLength(50)]
    public string? TransferConf2 { get; set; }
    
    /// <summary>
    /// Transfer conference number 3 override
    /// </summary>
    [StringLength(50)]
    public string? TransferConf3 { get; set; }
    
    /// <summary>
    /// Transfer conference number 4 override
    /// </summary>
    [StringLength(50)]
    public string? TransferConf4 { get; set; }
    
    /// <summary>
    /// Transfer conference number 5 override
    /// </summary>
    [StringLength(50)]
    public string? TransferConf5 { get; set; }
    
    /// <summary>
    /// Number of resets performed today
    /// </summary>
    public int ResetsToday { get; set; } = 0;
    
    /// <summary>
    /// Last reset date/time
    /// </summary>
    public DateTime? LastResetAt { get; set; }
    
    /// <summary>
    /// Tags for categorizing and filtering lists
    /// </summary>
    [StringLength(500)]
    public string? Tags { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual ICollection<Lead> Leads { get; set; } = new List<Lead>();
    public virtual ICollection<CampaignList> CampaignLists { get; set; } = new List<CampaignList>();
}
