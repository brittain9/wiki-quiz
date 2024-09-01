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
        };
    }

    public static SubmissionResponseDto ToDto(this Submission submission)
    {
        SubmissionResponseDto dto = new SubmissionResponseDto()
        {
            Id = submission.Id,
            Score = submission.Score,
            Title = submission.Quiz.Title,
            SubmissionTime = submission.SubmissionTime
        };
        
        return dto;
    }

    public static SubmissionDetailDto ToDetailedDto(this Submission submission)
    {
        return new SubmissionDetailDto()
        {
            Quiz = submission.Quiz.ToDto(),
            Answers = submission.Answers,
            Score = submission.Score,
            SubmissionTime = submission.SubmissionTime
        };
    }
}
