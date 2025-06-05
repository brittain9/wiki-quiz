using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WikiQuizGenerator.Core.Models;

public class Question
{
    public int Id { get; set; }

    [Required]
    public string Text { get; set; }

    // Questions will have max five choices.
    [Required]
    public string Option1 { get; set; }

    [Required]
    public string Option2 { get; set; }

    public string? Option3 { get; set; }

    public string? Option4 { get; set; }

    public string? Option5 { get; set; }

    [Range(1, 5)]
    public int CorrectOptionNumber { get; set; }

    public int PointValue { get; set; } = 1000; // Default points per question

    // Navigational property for one to many relationship between ai response and question
    public int AIResponseId { get; set; }
    
    [JsonIgnore]
    public AIResponse AIResponse { get; set; }
}
