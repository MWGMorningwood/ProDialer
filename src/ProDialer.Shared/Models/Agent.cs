using System.ComponentModel.DataAnnotations;

namespace ProDialer.Shared.Models;

public class Agent
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string FullName => $"{FirstName} {LastName}".Trim();
    
    [Required]
    [StringLength(200)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// Current status: Available, Busy, OnCall, Break, Offline, LoggedOut
    /// </summary>
    [StringLength(20)]
    public string Status { get; set; } = "LoggedOut";
    
    /// <summary>
    /// Whether the agent is currently logged into the system
    /// </summary>
    public bool IsLoggedIn { get; set; } = false;
    
    /// <summary>
    /// Whether the agent is currently on a call
    /// </summary>
    public bool IsOnCall { get; set; } = false;
    
    /// <summary>
    /// Number of active calls the agent is currently handling
    /// </summary>
    public int ActiveCalls { get; set; } = 0;
    
    /// <summary>
    /// Maximum number of concurrent calls this agent can handle
    /// </summary>
    [Range(1, 10)]
    public int MaxConcurrentCalls { get; set; } = 1;
    
    /// <summary>
    /// Agent's skill level (affects call routing and priorities)
    /// </summary>
    [Range(1, 10)]
    public int SkillLevel { get; set; } = 5;
    
    /// <summary>
    /// Campaigns this agent is qualified to handle (comma-separated campaign IDs or names)
    /// </summary>
    [StringLength(500)]
    public string? QualifiedCampaigns { get; set; }
    
    /// <summary>
    /// Languages the agent speaks (comma-separated language codes)
    /// </summary>
    [StringLength(200)]
    public string? Languages { get; set; }
    
    /// <summary>
    /// Agent's time zone
    /// </summary>
    [StringLength(100)]
    public string TimeZone { get; set; } = "UTC";
    
    /// <summary>
    /// When the agent logged in for the current session
    /// </summary>
    public DateTime? CurrentSessionStartedAt { get; set; }
    
    /// <summary>
    /// When the agent last logged out
    /// </summary>
    public DateTime? LastLoggedOutAt { get; set; }
    
    /// <summary>
    /// Total time logged in today (in minutes)
    /// </summary>
    public int TodayLoggedInMinutes { get; set; } = 0;
    
    /// <summary>
    /// Total calls handled today
    /// </summary>
    public int TodayCallsHandled { get; set; } = 0;
    
    /// <summary>
    /// Total talk time today (in minutes)
    /// </summary>
    public int TodayTalkTimeMinutes { get; set; } = 0;
    
    /// <summary>
    /// Role or department
    /// </summary>
    [StringLength(100)]
    public string? Role { get; set; }
    
    /// <summary>
    /// Manager or supervisor
    /// </summary>
    [StringLength(100)]
    public string? Supervisor { get; set; }
    
    /// <summary>
    /// Whether the agent is currently active/enabled
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Agent's extension number or SIP address for communication service
    /// </summary>
    [StringLength(100)]
    public string? Extension { get; set; }
    
    /// <summary>
    /// Communication service endpoint or identifier for this agent
    /// </summary>
    [StringLength(200)]
    public string? CommunicationEndpoint { get; set; }
    
    /// <summary>
    /// Tags for categorizing agents
    /// </summary>
    [StringLength(500)]
    public string? Tags { get; set; }
    
    /// <summary>
    /// Custom fields for agent-specific data (JSON format)
    /// </summary>
    public string? CustomFields { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;
}
