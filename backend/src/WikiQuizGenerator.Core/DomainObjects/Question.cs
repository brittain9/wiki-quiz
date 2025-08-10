namespace WikiQuizGenerator.Core.DomainObjects;

/// <summary>
/// Domain object representing a question with options and correct answer index
/// </summary>
public class Question
{
    public string Text { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int CorrectAnswerIndex { get; set; } // 0-based index for the correct option
}