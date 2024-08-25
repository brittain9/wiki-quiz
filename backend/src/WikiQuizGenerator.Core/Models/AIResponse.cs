using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WikiQuizGenerator.Core.Models;

public class AIResponse
{
    public int Id { get; set; }

    public IList<Question> Questions { get; set; }

    public long ResponseTime { get; set; } // in milliseconds
    public int? PromptTokenUsage { get; set; }
    public int? CompletionTokenUsage { get; set; }
    public string? ModelName { get; set; }

    // Navigational Property for one-to-one relationship between ai response and wikipedia page
    public int WikipediaPageId { get; set; }
    public WikipediaPage WikipediaPage { get; set; }

    // Navigational Property for one to many relationship between quiz and ai response.
    public int QuizId { get; set; }
    
    [JsonIgnore]
    public Quiz Quiz { get; set; }
}