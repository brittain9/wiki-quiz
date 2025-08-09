using System.Text.Json.Serialization;

namespace WikiQuizGenerator.Data.Cosmos.Models;

/// <summary>
/// Optimized Quiz document that embeds everything for single-read efficiency
/// Uses WikipediaReference instead of storing full Wikipedia data
/// </summary>
public class Quiz
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [JsonPropertyName("partitionKey")]
    public string PartitionKey { get; set; } = string.Empty; // CreatedBy user ID
    
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
    
    [JsonPropertyName("createdBy")]
    public Guid CreatedBy { get; set; }
    
    // Just store minimal Wikipedia reference - not the full page!
    [JsonPropertyName("wikipediaSource")]
    public WikipediaReference WikipediaSource { get; set; } = new();
    
    // Embed AIResponses directly - no separate documents
    [JsonPropertyName("aiResponses")]
    public List<AIResponse> AIResponses { get; set; } = new();
    
    // Single submission per quiz for now - can expand later for multiplayer
    [JsonPropertyName("submission")]
    public Submission? Submission { get; set; }
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = "quiz";
}