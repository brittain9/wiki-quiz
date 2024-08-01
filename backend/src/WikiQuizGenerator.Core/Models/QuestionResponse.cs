using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WikiQuizGenerator.Core.Models;

public class QuestionResponse
{
    [Key]
    public int Id { get; set; }
    public int? PromptTokenUsage { get; set; }
    public int? CompletionTokenUsage { get; set; }
    public long AIResponseTime { get; set; } // in milliseconds
    public string ModelName { get; set; }
    public int WikipediaPageId { get; set; }

    [ForeignKey("WikipediaPageId")]
    public WikipediaPage WikipediaPage { get; set; }
    public IList<Question> Questions { get; set; }

    public QuestionResponse()
    {
        Questions = new List<Question>();
    }

    public int? TotalTokens
    {
        get
        {
            if (PromptTokenUsage.HasValue && CompletionTokenUsage.HasValue)
            {
                return PromptTokenUsage.Value + CompletionTokenUsage.Value;
            }
            else if (PromptTokenUsage.HasValue)
            {
                return PromptTokenUsage.Value;
            }
            else if (CompletionTokenUsage.HasValue)
            {
                return CompletionTokenUsage.Value;
            }
            else
            {
                return null;
            }
        }
        
    }
}
