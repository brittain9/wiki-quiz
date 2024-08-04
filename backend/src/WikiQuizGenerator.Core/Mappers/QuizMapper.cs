using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WikiQuizGenerator.Core.DTOs;
using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Core.Mappers;

public class QuizMapper
{
    public static QuizDto ToDto(Quiz quiz)
    {
        var aiResponseDtos = new List<AIResponseDto>();
        foreach (var response in quiz.AIResponses)
        {
            aiResponseDtos.Add(AIResponseMapper.ToDto(response));
        }

        return new QuizDto
        {
            Id = quiz.Id,
            Title = quiz.Title,
            CreatedAt = quiz.CreatedAt,
            AIResponses = aiResponseDtos,
        };
    }
}
