using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WikiQuizGenerator.Core.Models;

public class Quiz
{
    public int Id { get; set; }
    public string Title { get; set; }
    public DateTime CreatedAt { get; set; }
    // These contain the questions and some extra information
    public IList<AIResponse> AIResponses { get; set; } = new List<AIResponse>();
    public IList<QuizSubmission> QuizSubmissions { get; set; } = new List<QuizSubmission>();
}