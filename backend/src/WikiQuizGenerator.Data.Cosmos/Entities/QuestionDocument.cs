namespace WikiQuizGenerator.Data.Cosmos.Entities;

public sealed class QuestionDocument
{
    public string Text { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int CorrectAnswerIndex { get; set; }
}


