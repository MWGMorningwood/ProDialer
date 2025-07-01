namespace ProDialer.Shared.DTOs;

// Lead Filter DTOs
public class CreateLeadFilterDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FilterType { get; set; } = "RULE_BASED";
    public string? SqlFilter { get; set; }
    public string? FilterRules { get; set; }
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 5;
    public string? UserGroup { get; set; }
}

public class UpdateLeadFilterDto : CreateLeadFilterDto
{
    public int Id { get; set; }
}

public class LeadFilterDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FilterType { get; set; } = "RULE_BASED";
    public string? SqlFilter { get; set; }
    public string? FilterRules { get; set; }
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public string? UserGroup { get; set; }
    public int MatchingLeadCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}

public class LeadFilterSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FilterType { get; set; } = "RULE_BASED";
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public int MatchingLeadCount { get; set; }
}
