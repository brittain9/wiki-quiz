using WikiQuizGenerator.Core.DTOs;
using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Core.Mappers;

public static class SubmissionMapper
{
    public static Submission ToModel(this SubmissionDto dto)
    {
        return new Submission
        {
            QuizId = dto.QuizId,
            Answers = dto.QuestionAnswers.Select(a => new QuestionAnswer
            {
                QuestionId = a.QuestionId,
                SelectedOptionNumber = a.SelectedOptionNumber
            }).ToList(),
            SubmissionTime = DateTime.UtcNow
            // Note: UserId is set in the controller from ClaimsPrincipal
            // User navigation property will be populated by Entity Framework
        };
    }

    public static SubmissionResponseDto ToDto(this Submission submission)
    {
        SubmissionResponseDto dto = new SubmissionResponseDto()
        {
            Id = submission.Id,
            Score = submission.Score,
            Title = submission.Quiz.Title,
            UserId = submission.UserId,
            SubmissionTime = submission.SubmissionTime
        };
        
        return dto;
    }
}
