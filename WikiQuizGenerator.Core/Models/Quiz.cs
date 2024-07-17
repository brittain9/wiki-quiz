namespace WikiQuizGenerator.Core.Models;

public class Quiz
{
    public int Id { get; set; }
    public string Title { get; set; }
    public List<Question> Questions { get; set; }
}