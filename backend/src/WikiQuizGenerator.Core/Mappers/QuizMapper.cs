using WikiQuizGenerator.Core.DTOs;
using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Core.Mappers;

public static class QuizMapper
{
    public static QuizDto ToDto(this Quiz quiz)
    {
        var aiResponseDtos = new List<AIResponseDto>();
        foreach (var response in quiz.AIResponses)
        {
            aiResponseDtos.Add(response.ToDto());
        }

        return new QuizDto
        {
            Id = quiz.Id,
            Title = quiz.Title,
            CreatedAt = quiz.CreatedAt,
            AIResponses = aiResponseDtos,
        };
    }

    public static AIResponseDto ToDto(this AIResponse aiResponse)
    {
        var questionsDtos = new List<QuestionDto>();
        foreach (var question in aiResponse.Questions)
        {
            questionsDtos.Add(question.ToDto());
        }

        return new AIResponseDto
        {
            Id = aiResponse.Id,
            ResponseTopic = aiResponse.WikipediaPage.Title,
            TopicUrl = aiResponse.WikipediaPage.Url,
            ResponseTime = aiResponse.ResponseTime,
            PromptTokenUsage = aiResponse.PromptTokenUsage,
            CompletionTokenUsage = aiResponse.CompletionTokenUsage,
            ModelName = aiResponse.ModelName,
            Questions = questionsDtos
        };
    }

    public static QuestionDto ToDto(this Question question)
    {
        var options = new List<string>
        {
            question.Option1,
            question.Option2
        };

        if (!string.IsNullOrEmpty(question.Option3))
            options.Add(question.Option3);
        if (!string.IsNullOrEmpty(question.Option4))
            options.Add(question.Option4);
        if (!string.IsNullOrEmpty(question.Option5))
            options.Add(question.Option5);

        return new QuestionDto
        {
            Id = question.Id,
            Text = question.Text,
            Options = options
        };
    }
}
