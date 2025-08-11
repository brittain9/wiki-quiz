namespace WikiQuizGenerator.Core.DomainObjects;

/// <summary>
/// Domain object representing a quiz - simplified for Cosmos DB
/// </summary>
public class Quiz
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public List<Question> Questions { get; set; } = new();
    public WikipediaReference? WikipediaReference { get; set; }
    public Submission? Submission { get; set; }

    // Usage/cost tracking
    public int? InputTokenCount { get; set; }
    public int? OutputTokenCount { get; set; }
    public long? ResponseTimeMs { get; set; }
    public string? ModelId { get; set; }
    public double? EstimatedCostUsd { get; set; }
}