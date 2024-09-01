using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WikiQuizGenerator.Core.Models;

public class Quiz
{
    public int Id { get; set; }
    public string Title { get; set; }
    public DateTime CreatedAt { get; set; }
    // These contain the questions and some extra information
    public IList<AIResponse> AIResponses { get; set; } = new List<AIResponse>();
    
    [JsonIgnore]
    public IList<Submission> QuizSubmissions { get; set; } = new List<Submission>();
}