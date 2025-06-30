using System.ComponentModel.DataAnnotations;

namespace ProDialer.Shared.Models;

/// <summary>
/// Junction table for many-to-many relationship between Campaigns and Lists
/// </summary>
public class CampaignList
{
    public int Id { get; set; }
    
    public int CampaignId { get; set; }
    public int ListId { get; set; }
    
    /// <summary>
    /// Priority of this list within the campaign (higher number = higher priority)
    /// </summary>
    [Range(1, 10)]
    public int Priority { get; set; } = 5;
    
    /// <summary>
    /// Whether this list is currently active in the campaign
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Percentage of total campaign capacity allocated to this list
    /// </summary>
    [Range(0, 100)]
    public int AllocationPercentage { get; set; } = 100;
    
    /// <summary>
    /// Maximum number of calls per hour for this list in this campaign
    /// </summary>
    [Range(0, 1000)]
    public int? MaxCallsPerHour { get; set; }
    
    /// <summary>
    /// When this list was added to the campaign
    /// </summary>
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Who added this list to the campaign
    /// </summary>
    [StringLength(100)]
    public string AddedBy { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual Campaign Campaign { get; set; } = null!;
    public virtual List List { get; set; } = null!;
}
