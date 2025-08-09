using System.Text.Json.Serialization;

namespace WikiQuizGenerator.Data.Cosmos.Models;

public class Question
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
    
    // Much better than Option1, Option2, Option3, Option4, Option5!
    // This is cleaner, more flexible, and better for JSON serialization
    [JsonPropertyName("options")]
    public List<string> Options { get; set; } = new();
    
    [JsonPropertyName("correctOptionNumber")]
    public int CorrectOptionNumber { get; set; } // Keep your 1-based indexing for compatibility
    
    [JsonPropertyName("pointValue")]
    public int PointValue { get; set; } = 1000;
}