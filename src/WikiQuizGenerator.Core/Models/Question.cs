namespace WikiQuizGenerator.Core.Models;

public class Question
{
    public string Text { get; set; }
    public List<string> Options { get; set; }
    public int CorrectAnswerIndex { get; set; }
}