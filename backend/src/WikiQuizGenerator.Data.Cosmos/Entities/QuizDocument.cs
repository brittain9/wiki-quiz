using System.Text.Json.Serialization;
using WikiQuizGenerator.Core.DomainObjects;

namespace WikiQuizGenerator.Data.Cosmos.Entities;

public sealed class QuizDocument
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("partitionKey")] public string PartitionKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public WikipediaReference? WikipediaReference { get; set; }
    public List<QuestionDocument> Questions { get; set; } = new();
    public SubmissionDocument? Submission { get; set; }
    public int? InputTokenCount { get; set; }
    public int? OutputTokenCount { get; set; }
    public long? ResponseTimeMs { get; set; }
    public string? ModelId { get; set; }
    public double? EstimatedCostUsd { get; set; }
}


