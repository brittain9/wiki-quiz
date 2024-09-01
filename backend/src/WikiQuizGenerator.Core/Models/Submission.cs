using System.Text.Json.Serialization;

namespace WikiQuizGenerator.Core.Models;

public class Submission
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    
    public Quiz Quiz { get; set; }
    public List<QuestionAnswer> Answers { get; set; } = new List<QuestionAnswer>();
    public DateTime SubmissionTime { get; set; }
    public int Score { get; set; }
}