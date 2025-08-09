using System.Text.Json.Serialization;

namespace WikiQuizGenerator.Data.Cosmos.Models;

public class Submission
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }
    
    [JsonPropertyName("submissionTime")]
    public DateTime SubmissionTime { get; set; }
    
    [JsonPropertyName("score")]
    public int Score { get; set; }
    
    [JsonPropertyName("pointsEarned")]
    public int PointsEarned { get; set; }
    
    // Embed answers directly
    [JsonPropertyName("answers")]
    public List<QuestionAnswer> Answers { get; set; } = new();
}