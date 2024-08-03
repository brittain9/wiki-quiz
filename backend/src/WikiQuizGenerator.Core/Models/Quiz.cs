using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WikiQuizGenerator.Core.Models;

public class Quiz
{
    [Key]
    public int Id { get; set; }
    public string Title { get; set; }

    [Column(TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; }

    public IList<AIResponse> AIResponses { get; set; }
    
    // Add an array for the user's answer choices gotten on post method.
    // add an array for the correct answer for easy checking?
    // need another DTO for completed quiz

    // Add a user later

    public Quiz()
    {
        AIResponses = new List<AIResponse>();
    }
}