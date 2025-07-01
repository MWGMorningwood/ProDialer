using System.ComponentModel.DataAnnotations;

namespace ProDialer.Shared.Models;

/// <summary>
/// Disposition codes for call outcomes
/// </summary>
public class Disposition
{
    public int Id { get; set; }
    
    /// <summary>
    /// Short code for the disposition (e.g., "SALE", "NI", "CB")
    /// </summary>
    [Required]
    [StringLength(6)]
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Human-readable name for the disposition
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Detailed description of when to use this disposition
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Category: SALE, CONTACT, NO_CONTACT, CALLBACK, DNC, etc.
    /// </summary>
    [StringLength(20)]
    public string Category { get; set; } = "CONTACT";
    
    /// <summary>
    /// Whether this disposition is selectable by agents
    /// </summary>
    public bool IsSelectable { get; set; } = true;
    
    /// <summary>
    /// Whether this disposition counts as a successful contact
    /// </summary>
    public bool IsContact { get; set; } = true;
    
    /// <summary>
    /// Whether this disposition indicates a sale
    /// </summary>
    public bool IsSale { get; set; } = false;
    
    /// <summary>
    /// Whether this disposition requires a callback to be scheduled
    /// </summary>
    public bool RequiresCallback { get; set; } = false;
    
    /// <summary>
    /// Whether this disposition adds the lead to DNC list
    /// </summary>
    public bool AddToDoNotCall { get; set; } = false;
    
    /// <summary>
    /// Campaign this disposition belongs to (null for global dispositions)
    /// </summary>
    public int? CampaignId { get; set; }
    
    /// <summary>
    /// Display order in agent interface
    /// </summary>
    public int DisplayOrder { get; set; } = 0;
    
    /// <summary>
    /// Hot key for quick selection (single character)
    /// </summary>
    [StringLength(1)]
    public string? HotKey { get; set; }
    
    /// <summary>
    /// Background color for this disposition in UI (hex color)
    /// </summary>
    [StringLength(7)]
    public string? BackgroundColor { get; set; }
    
    /// <summary>
    /// Whether this disposition is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual Campaign? Campaign { get; set; }
    public virtual ICollection<CallLog> CallLogs { get; set; } = new List<CallLog>();
}
