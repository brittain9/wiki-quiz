using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WikiQuizGenerator.Core.Models;

public class Question
{
    [Key]
    public int Id { get; set; }
    public string Text { get; set; }
    public List<string> Options { get; set; }
    public int CorrectAnswerIndex { get; set; }
    public int QuestionResponseId { get; set; }

    [ForeignKey("QuestionResponseId")]
    public QuestionResponse QuestionResponse { get; set; }
}