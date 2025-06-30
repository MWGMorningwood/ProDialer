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
    /// Whether leads from this list should be called in order or randomly
    /// </summary>
    public bool CallInOrder { get; set; } = true;
    
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
    /// Tags for categorizing and filtering lists
    /// </summary>
    [StringLength(500)]
    public string? Tags { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual ICollection<Lead> Leads { get; set; } = new List<Lead>();
    public virtual ICollection<CampaignList> CampaignLists { get; set; } = new List<CampaignList>();
}
