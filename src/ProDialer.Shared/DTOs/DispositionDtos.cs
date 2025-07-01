namespace ProDialer.Shared.DTOs;

// Disposition Category DTOs
public class CreateDispositionCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Color { get; set; } = "#808080";
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}

public class UpdateDispositionCategoryDto : CreateDispositionCategoryDto
{
    public int Id { get; set; }
}

public class DispositionCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Color { get; set; } = "#808080";
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// Disposition Code DTOs
public class CreateDispositionCodeDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Code { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public bool IsContact { get; set; } = false;
    public bool IsSale { get; set; } = false;
    public bool ShouldRecycle { get; set; } = false;
    public int RecycleDelayHours { get; set; } = 24;
    public bool RequiresCallback { get; set; } = false;
    public string? RequiredFields { get; set; }
    public string? AutoActions { get; set; }
    public string? HotKey { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}

public class UpdateDispositionCodeDto : CreateDispositionCodeDto
{
    public int Id { get; set; }
}

public class DispositionCodeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Code { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public bool IsContact { get; set; }
    public bool IsSale { get; set; }
    public bool ShouldRecycle { get; set; }
    public int RecycleDelayHours { get; set; }
    public bool RequiresCallback { get; set; }
    public string? RequiredFields { get; set; }
    public string? AutoActions { get; set; }
    public string? HotKey { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class DispositionCodeSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public bool IsContact { get; set; }
    public bool IsSale { get; set; }
    public bool IsActive { get; set; }
}
