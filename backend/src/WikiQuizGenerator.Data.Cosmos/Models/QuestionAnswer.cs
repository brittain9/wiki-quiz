using System.Text.Json.Serialization;

namespace WikiQuizGenerator.Data.Cosmos.Models;

public class QuestionAnswer
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("questionId")]
    public int QuestionId { get; set; }
    
    [JsonPropertyName("selectedOptionNumber")]
    public int SelectedOptionNumber { get; set; }
}