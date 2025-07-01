using System.ComponentModel.DataAnnotations;

namespace ProDialer.Shared.Models;

/// <summary>
/// VICIdial-style lead filtering system for advanced lead selection
/// </summary>
public class LeadFilter
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Filter type: SQL, RULE_BASED, FIELD_BASED
    /// </summary>
    [StringLength(50)]
    public string FilterType { get; set; } = "RULE_BASED";
    
    /// <summary>
    /// SQL WHERE clause for SQL-based filters
    /// </summary>
    [StringLength(2000)]
    public string? SqlFilter { get; set; }
    
    /// <summary>
    /// Rule-based filter definition (JSON format)
    /// Example: {"rules": [{"field": "state", "operator": "IN", "value": ["CA", "NY"]}, {"field": "age", "operator": ">=", "value": 18}]}
    /// </summary>
    public string? FilterRules { get; set; }
    
    /// <summary>
    /// Whether this filter is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Filter priority (higher numbers processed first)
    /// </summary>
    [Range(1, 10)]
    public int Priority { get; set; } = 5;
    
    /// <summary>
    /// User group that can use this filter
    /// </summary>
    [StringLength(100)]
    public string? UserGroup { get; set; }
    
    /// <summary>
    /// Stats: How many leads match this filter
    /// </summary>
    public int MatchingLeadCount { get; set; } = 0;
    
    /// <summary>
    /// Last time this filter was applied
    /// </summary>
    public DateTime? LastAppliedAt { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();
}
