using System.ComponentModel.DataAnnotations;

namespace WikiQuizGenerator.Core.Models;

public class Quiz
{
    [Key]
    public int Id { get; set; }
    public string Title { get; set; }
    // Question responses is a wrapper to include metadata about token usage and the topic if it is a link of the user's topic
    public List<QuestionResponse> QuestionResponses { get; set; }
    // Add a User later

    public Quiz()
    {
        QuestionResponses = new List<QuestionResponse>();
    }
}