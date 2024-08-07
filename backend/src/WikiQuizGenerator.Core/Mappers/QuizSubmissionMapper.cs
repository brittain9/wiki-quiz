using WikiQuizGenerator.Core.DTOs;
using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Core.Mappers;

public static class QuizSubmissionMapper
{
    public static QuizSubmission ToModel(this QuizSubmissionDto dto)
    {
        return new QuizSubmission
        {
            QuizId = dto.QuizId,
            Answers = dto.QuestionAnswers.Select(a => new QuestionAnswer
            {
                QuestionId = a.QuestionId,
                SelectedOptionNumber = a.SelectedOptionNumber
            }).ToList(),
            SubmissionTime = DateTime.UtcNow
        };
    }
}
