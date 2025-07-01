using System.ComponentModel.DataAnnotations;

namespace ProDialer.Shared.Models;

/// <summary>
/// VICIdial-style hierarchical disposition system
/// </summary>
public class DispositionCategory
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Category code (e.g., "CONTACT", "NO_CONTACT", "SALE")
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Category color for UI display
    /// </summary>
    [StringLength(7)]
    public string Color { get; set; } = "#808080";
    
    /// <summary>
    /// Display order
    /// </summary>
    public int DisplayOrder { get; set; } = 0;
    
    /// <summary>
    /// Whether this category is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<DispositionCode> DispositionCodes { get; set; } = new List<DispositionCode>();
}

/// <summary>
/// Individual disposition codes within categories
/// </summary>
public class DispositionCode
{
    public int Id { get; set; }
    
    /// <summary>
    /// Category this disposition belongs to
    /// </summary>
    public int CategoryId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Disposition code (e.g., "SALE", "NI", "CB", "DNC")
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether this is a contact (successful connection)
    /// </summary>
    public bool IsContact { get; set; } = false;
    
    /// <summary>
    /// Whether this is a sale/conversion
    /// </summary>
    public bool IsSale { get; set; } = false;
    
    /// <summary>
    /// Whether lead should be recycled with this disposition
    /// </summary>
    public bool ShouldRecycle { get; set; } = false;
    
    /// <summary>
    /// Hours to wait before recycling (if ShouldRecycle = true)
    /// </summary>
    public int RecycleDelayHours { get; set; } = 24;
    
    /// <summary>
    /// Whether to schedule a callback
    /// </summary>
    public bool RequiresCallback { get; set; } = false;
    
    /// <summary>
    /// Required form fields (JSON array): ["notes", "callback_date", "amount"]
    /// </summary>
    public string? RequiredFields { get; set; }
    
    /// <summary>
    /// Auto-actions to perform (JSON): {"send_email": true, "update_crm": true}
    /// </summary>
    public string? AutoActions { get; set; }
    
    /// <summary>
    /// Hot key for quick selection (single character)
    /// </summary>
    [StringLength(1)]
    public string? HotKey { get; set; }
    
    /// <summary>
    /// Display order within category
    /// </summary>
    public int DisplayOrder { get; set; } = 0;
    
    /// <summary>
    /// Whether this disposition is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Usage statistics
    /// </summary>
    public int UsageCount { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual DispositionCategory Category { get; set; } = null!;
    public virtual ICollection<CallLog> CallLogs { get; set; } = new List<CallLog>();
}
