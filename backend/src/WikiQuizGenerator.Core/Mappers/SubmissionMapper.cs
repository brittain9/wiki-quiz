using WikiQuizGenerator.Core.DTOs;
using WikiQuizGenerator.Core.DomainObjects;

namespace WikiQuizGenerator.Core.Mappers;

public static class SubmissionMapper
{
    public static Submission ToModel(this SubmissionDto dto)
    {
        return new Submission
        {
            QuizId = dto.QuizId,
            UserId = Guid.Empty,
            Answers = dto.QuestionAnswers.Select(a => a.SelectedOptionNumber).ToList(),
            SubmissionTime = DateTime.UtcNow
        };
    }

    public static SubmissionResponseDto ToDto(this Submission submission)
    {
        SubmissionResponseDto dto = new SubmissionResponseDto()
        {
            Id = submission.QuizId,
            Score = submission.Score,
            PointsEarned = submission.PointsEarned,
            Title = submission.Title,
            UserId = submission.UserId,
            SubmissionTime = submission.SubmissionTime
        };
        
        return dto;
    }
}
