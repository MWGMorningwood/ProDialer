namespace ProDialer.Shared.DTOs;

// DNC List DTOs
public class CreateDncListDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ListType { get; set; } = "INTERNAL";
    public bool IsActive { get; set; } = true;
    public string Scope { get; set; } = "SYSTEM_WIDE";
    public string Source { get; set; } = "MANUAL";
    public bool AutoScrubbing { get; set; } = true;
    public int? CampaignId { get; set; }
    public int? ListId { get; set; }
}

public class UpdateDncListDto : CreateDncListDto
{
    public int Id { get; set; }
}

public class DncListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ListType { get; set; } = "INTERNAL";
    public bool IsActive { get; set; }
    public string Scope { get; set; } = "SYSTEM_WIDE";
    public int TotalNumbers { get; set; }
    public string Source { get; set; } = "MANUAL";
    public bool AutoScrubbing { get; set; }
    public int? CampaignId { get; set; }
    public string? CampaignName { get; set; }
    public int? ListId { get; set; }
    public string? ListName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}

public class DncListSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ListType { get; set; } = "INTERNAL";
    public bool IsActive { get; set; }
    public string Scope { get; set; } = "SYSTEM_WIDE";
    public int TotalNumbers { get; set; }
    public string Source { get; set; } = "MANUAL";
}

// DNC Number DTOs
public class CreateDncNumberDto
{
    public int DncListId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string PhoneCode { get; set; } = "1";
    public string Reason { get; set; } = "OPT_OUT";
    public string? Notes { get; set; }
}

public class DncNumberDto
{
    public int Id { get; set; }
    public int DncListId { get; set; }
    public string? DncListName { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string PhoneCode { get; set; } = "1";
    public string Reason { get; set; } = "OPT_OUT";
    public DateTime AddedAt { get; set; }
    public string? Notes { get; set; }
    public string AddedBy { get; set; } = string.Empty;
}

public class BulkDncImportDto
{
    public int DncListId { get; set; }
    public List<string> PhoneNumbers { get; set; } = new();
    public string Reason { get; set; } = "OPT_OUT";
    public string? Notes { get; set; }
}

public class DncCheckDto
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string PhoneCode { get; set; } = "1";
    public int? CampaignId { get; set; }
    public int? ListId { get; set; }
}

public class DncCheckResultDto
{
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsOnDnc { get; set; }
    public List<DncMatchDto> Matches { get; set; } = new();
}

public class DncMatchDto
{
    public int DncListId { get; set; }
    public string DncListName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; }
}
