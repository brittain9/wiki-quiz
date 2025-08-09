using System.Text.Json.Serialization;

namespace WikiQuizGenerator.Data.Cosmos.Models;

public class AIResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("responseTime")]
    public long ResponseTime { get; set; }
    
    [JsonPropertyName("inputTokenCount")]
    public int? InputTokenCount { get; set; }
    
    [JsonPropertyName("outputTokenCount")]
    public int? OutputTokenCount { get; set; }
    
    [JsonPropertyName("modelConfigId")]
    public int ModelConfigId { get; set; }
    
    // No WikipediaPage storage here - we reference it from the parent Quiz!
    // This eliminates massive data duplication
    
    [JsonPropertyName("questions")]
    public List<Question> Questions { get; set; } = new();
}