namespace WikiQuizGenerator.Core.Models;

public class QuizSubmission
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public Quiz Quiz { get; set; }
    public List<QuestionAnswer> Answers { get; set; } = new List<QuestionAnswer>();
    public DateTime SubmissionTime { get; set; }
}