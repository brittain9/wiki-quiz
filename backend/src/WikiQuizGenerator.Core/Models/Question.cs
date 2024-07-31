using System.ComponentModel.DataAnnotations;

namespace WikiQuizGenerator.Core.Models;

public class Question
{
    [Key]
    public int Id { get; set; }
    public string Text { get; set; }
    public List<string> Options { get; set; }
    public int CorrectAnswerIndex { get; set; }
}