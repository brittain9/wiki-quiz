using System.ComponentModel.DataAnnotations;

namespace WikiQuizGenerator.Core.Models;

public class Quiz
{
    [Key]
    public int Id { get; set; }
    public string Title { get; set; }
    public DateTime CreatedAt { get; set; }

    public IList<AIResponse> AIResponses { get; set; }

    // Add a user later

    public Quiz()
    {
        AIResponses = new List<AIResponse>();
    }
}