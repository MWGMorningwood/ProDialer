namespace ProDialer.Shared.DTOs;

/// <summary>
/// DTO for starting live transcription
/// </summary>
public class StartTranscriptionRequest
{
    public string CallId { get; set; } = string.Empty;
    public string? Language { get; set; }
    public bool EnableRealTime { get; set; } = true;
}

/// <summary>
/// DTO for transcription session response
/// </summary>
public class TranscriptionSessionDto
{
    public string SessionId { get; set; } = string.Empty;
    public string CallId { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string? Language { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// DTO for live transcription updates
/// </summary>
public class LiveTranscriptionUpdateDto
{
    public string SessionId { get; set; } = string.Empty;
    public string CallId { get; set; } = string.Empty;
    public string ChunkId { get; set; } = string.Empty;
    public string PartialText { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public bool IsComplete { get; set; }
    public DateTime Timestamp { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// DTO for final transcription results
/// </summary>
public class TranscriptionResultDto
{
    public string CallId { get; set; } = string.Empty;
    public string? SessionId { get; set; }
    public bool Success { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? Language { get; set; }
    public double Duration { get; set; }
    public double Confidence { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ProcessedAt { get; set; }
}

/// <summary>
/// DTO for transcription settings
/// </summary>
public class TranscriptionSettingsDto
{
    public bool EnableByDefault { get; set; } = true;
    public string DefaultLanguage { get; set; } = "en";
    public bool RealTimeProcessing { get; set; } = true;
    public double MinConfidenceThreshold { get; set; } = 0.7;
    public int MaxTextLength { get; set; } = 10000;
}

/// <summary>
/// DTO for transcription statistics
/// </summary>
public class TranscriptionStatsDto
{
    public int TotalTranscriptions { get; set; }
    public int SuccessfulTranscriptions { get; set; }
    public int FailedTranscriptions { get; set; }
    public double AverageConfidence { get; set; }
    public double AverageDuration { get; set; }
    public Dictionary<string, int> LanguageDistribution { get; set; } = new();
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}