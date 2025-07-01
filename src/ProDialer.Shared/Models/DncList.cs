using System.ComponentModel.DataAnnotations;

namespace ProDialer.Shared.Models;

/// <summary>
/// VICIdial-style Do-Not-Call (DNC) list management
/// </summary>
public class DncList
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// DNC list type: INTERNAL, FEDERAL, STATE, CUSTOM
    /// </summary>
    [StringLength(50)]
    public string ListType { get; set; } = "INTERNAL";
    
    /// <summary>
    /// Whether this DNC list is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Scope: SYSTEM_WIDE, CAMPAIGN_SPECIFIC, LIST_SPECIFIC
    /// </summary>
    [StringLength(50)]
    public string Scope { get; set; } = "SYSTEM_WIDE";
    
    /// <summary>
    /// Campaign ID if campaign-specific
    /// </summary>
    public int? CampaignId { get; set; }
    
    /// <summary>
    /// List ID if list-specific
    /// </summary>
    public int? ListId { get; set; }
    
    /// <summary>
    /// Total phone numbers in this DNC list
    /// </summary>
    public int TotalNumbers { get; set; } = 0;
    
    /// <summary>
    /// Last time DNC list was updated
    /// </summary>
    public DateTime? LastUpdated { get; set; }
    
    /// <summary>
    /// Source of DNC data: UPLOAD, API, MANUAL, FEDERAL_REGISTRY
    /// </summary>
    [StringLength(100)]
    public string Source { get; set; } = "MANUAL";
    
    /// <summary>
    /// Expiration date for DNC entries (if applicable)
    /// </summary>
    public DateTime? ExpirationDate { get; set; }
    
    /// <summary>
    /// Auto-scrubbing enabled
    /// </summary>
    public bool AutoScrubbing { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual Campaign? Campaign { get; set; }
    public virtual List? List { get; set; }
    public virtual ICollection<DncNumber> DncNumbers { get; set; } = new List<DncNumber>();
}

/// <summary>
/// Individual phone number in a DNC list
/// </summary>
public class DncNumber
{
    public int Id { get; set; }
    
    /// <summary>
    /// DNC List this number belongs to
    /// </summary>
    public int DncListId { get; set; }
    
    /// <summary>
    /// Phone number (normalized format)
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
    /// Reason for DNC: OPT_OUT, COMPLAINT, FEDERAL_REGISTRY, etc.
    /// </summary>
    [StringLength(100)]
    public string Reason { get; set; } = "OPT_OUT";
    
    /// <summary>
    /// Date when number was added to DNC
    /// </summary>
    public DateTime AddedAt { get; set; }
    
    /// <summary>
    /// Expiration date for this DNC entry
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// Additional notes about this DNC entry
    /// </summary>
    [StringLength(500)]
    public string? Notes { get; set; }
    
    [StringLength(100)]
    public string AddedBy { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual DncList DncList { get; set; } = null!;
}
