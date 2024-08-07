namespace WikiQuizGenerator.Core.Models;

public class QuestionAnswer
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public int SelectedOptionNumber { get; set; }
}
