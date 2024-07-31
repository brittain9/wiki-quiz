using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WikiQuizGenerator.Core.Models;

public class QuestionResponse
{
    [Key]
    public int Id { get; set; }
    public string ResponseTopic { get; set; }
    public string TopicUrl { get; set; }
    public int? PromptTokenUsage { get; set; }
    public int? CompletionTokenUsage { get; set; }
    public long AIResponseTime { get; set; } // in milliseconds
    public string ModelName { get; set; }
    public List<Question> Questions { get; set; }

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
