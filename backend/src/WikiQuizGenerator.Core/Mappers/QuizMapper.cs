using WikiQuizGenerator.Core.DTOs;
using WikiQuizGenerator.Core.DomainObjects;

namespace WikiQuizGenerator.Core.Mappers;

public static class QuizMapper
{
    public static QuizDto ToDto(this Quiz quiz)
    {
        // Create a single AIResponse DTO from the simplified quiz structure
        var aiResponseDto = new AIResponseDto
        {
            Id = 1, // Simple ID since we have one response per quiz
            ResponseTopic = quiz.WikipediaReference?.Title ?? string.Empty,
            TopicUrl = quiz.WikipediaReference?.Url ?? string.Empty,
            Questions = quiz.Questions.Select((question, index) => question.ToDto(index + 1)).ToList()
        };

        return new QuizDto
        {
            Id = 1, // Simplified ID
            Title = quiz.Title,
            CreatedAt = quiz.CreatedAt,
            AIResponses = new List<AIResponseDto> { aiResponseDto }
        };
    }

    public static QuestionDto ToDto(this Question question, int id)
    {
        return new QuestionDto
        {
            Id = id,
            Text = question.Text,
            Options = question.Options
        };
    }
}
