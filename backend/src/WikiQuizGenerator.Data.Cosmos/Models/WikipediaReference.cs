using System.Text.Json.Serialization;

namespace WikiQuizGenerator.Data.Cosmos.Models;

/// <summary>
/// Minimal Wikipedia reference - just what we need for display
/// No need to store full Wikipedia page data in every quiz!
/// </summary>
public class WikipediaReference
{
    [JsonPropertyName("pageId")]
    public int PageId { get; set; }
    
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
    
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
    
    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;
}