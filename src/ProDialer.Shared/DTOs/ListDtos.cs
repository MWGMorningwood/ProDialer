namespace ProDialer.Shared.DTOs;

public class CreateListDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Priority { get; set; } = 5;
    public bool CallInOrder { get; set; } = true;
    public int? MaxCallAttempts { get; set; }
    public int? MinCallInterval { get; set; }
    public string? TimeZoneOverride { get; set; }
    public string? CallStartTimeOverride { get; set; }
    public string? CallEndTimeOverride { get; set; }
    public string? AllowedDaysOverride { get; set; }
    public bool IsActive { get; set; } = true;
    public string Source { get; set; } = "Manual";
    public string? SourceReference { get; set; }
    public string? CustomFieldsSchema { get; set; }
    public string? Tags { get; set; }
}

public class UpdateListDto : CreateListDto
{
    public int Id { get; set; }
}

public class ListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Priority { get; set; }
    public bool CallInOrder { get; set; }
    public int? MaxCallAttempts { get; set; }
    public int? MinCallInterval { get; set; }
    public string? TimeZoneOverride { get; set; }
    public string? CallStartTimeOverride { get; set; }
    public string? CallEndTimeOverride { get; set; }
    public string? AllowedDaysOverride { get; set; }
    public bool IsActive { get; set; }
    public string Source { get; set; } = string.Empty;
    public string? SourceReference { get; set; }
    public int TotalLeads { get; set; }
    public int CalledLeads { get; set; }
    public int ContactedLeads { get; set; }
    public string? CustomFieldsSchema { get; set; }
    public string? Tags { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}

public class ListSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int TotalLeads { get; set; }
    public int CalledLeads { get; set; }
    public int ContactedLeads { get; set; }
}
