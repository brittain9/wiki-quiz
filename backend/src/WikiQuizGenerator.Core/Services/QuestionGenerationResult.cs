using WikiQuizGenerator.Core.DomainObjects;

namespace WikiQuizGenerator.Core.Services;

/// <summary>
/// Result returned by question generation containing questions and metadata
/// </summary>
public class QuestionGenerationResult
{
    public List<Question> Questions { get; set; } = new();
    public float? ResponseTimeMs { get; set; }
    public int? InputTokenCount { get; set; }
    public int? OutputTokenCount { get; set; }
    public string? ModelId { get; set; }
    public double? EstimatedCostUsd { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}