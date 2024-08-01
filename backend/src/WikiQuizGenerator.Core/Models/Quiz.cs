using System.ComponentModel.DataAnnotations;

namespace WikiQuizGenerator.Core.Models;

public class Quiz
{
    [Key]
    public int Id { get; set; }
    public string Title { get; set; }
    public IList<QuestionResponse> QuestionResponses { get; set; }

    // Add a user later

    public Quiz()
    {
        QuestionResponses = new List<QuestionResponse>();
    }
}