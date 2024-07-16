namespace WikiQuizGenerator.Core.Models;

public class QuizQuestion
{
    public string Question { get; set; }
    public List<string> Options { get; set; }
    public int CorrectAnswerIndex { get; set; }
}