using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WikiQuizGenerator.Core.Models;

public class QuestionResponse
{
    // In our basic quiz these will be the same as the user's topic
    public string ResponseTopic { get; set; }
    public string TopicUrl { get; set; } // to add urls for user's exploration if a question piques their interest
    public int PromptTokenUsage { get; set; }
    public int CompletionTokenUsage { get; set; }
    public long AIResponseTime { get; set; } // in milliseconds
    public string ModelName { get; set; }
    public List<Question> Questions { get; set; }

    public QuestionResponse()
    {
        Questions = new List<Question>();
    }

    // make into property
    public int GetTotalTokens(){
        return PromptTokenUsage + CompletionTokenUsage;
    }
}
