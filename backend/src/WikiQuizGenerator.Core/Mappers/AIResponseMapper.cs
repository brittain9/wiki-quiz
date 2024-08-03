using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WikiQuizGenerator.Core.DTOs;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Core.Mappers;

internal class AIResponseMapper
{
    public static AIResponseDto ToDto(AIResponse aiResponse)
    {
        var questionsDtos = new List<QuestionDto>();
        foreach (var question in aiResponse.Questions)
        {
            questionsDtos.Add(QuestionMapper.ToDto(question));
        }

        return new AIResponseDto
        {
            ResponseTopic = aiResponse.WikipediaPage.Title,
            TopicUrl = aiResponse.WikipediaPage.Url,
            ResponseTime = aiResponse.AIMetadata.ResponseTime,
            PromptTokenUsage = aiResponse.AIMetadata.PromptTokenUsage,
            CompletionTokenUsage = aiResponse.AIMetadata.CompletionTokenUsage,
            ModelName = aiResponse.AIMetadata.ModelName,
            Questions = questionsDtos
        };
    }
}
