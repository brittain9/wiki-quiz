using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WikiQuizGenerator.Core.DTOs;

public class QuestionResponseDTO
{
    public string ResponseTopic { get; set; }
    public string TopicUrl { get; set; } // to add urls for user's exploration if a question piques their interest
    public List<QuestionDTO> Questions { get; set; }

    public QuestionResponseDTO()
    {
        Questions = new List<QuestionDTO>();
    }
}
