namespace WikiQuizGenerator.Core.DTOs;

public class QuestionDto
{
    public string Text { get; set; }
    public IList<string> Options { get; set; }
}