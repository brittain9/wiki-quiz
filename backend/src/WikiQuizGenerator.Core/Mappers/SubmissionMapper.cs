using WikiQuizGenerator.Core.DTOs;
using WikiQuizGenerator.Core.DomainObjects;

namespace WikiQuizGenerator.Core.Mappers;

public static class SubmissionMapper
{
    public static Submission ToModel(this SubmissionDto dto)
    {
        return new Submission
        {
            UserId = Guid.Empty, // This will be set by the controller from ClaimsPrincipal
            Answers = dto.QuestionAnswers.Select(a => a.SelectedOptionNumber).ToList(),
            SubmissionTime = DateTime.UtcNow
        };
    }

    public static SubmissionResponseDto ToDto(this Submission submission)
    {
        SubmissionResponseDto dto = new SubmissionResponseDto()
        {
            Id = 1, // Simplified ID
            Score = submission.Score,
            PointsEarned = 0, // Calculate based on score if needed
            Title = "", // Will need to be set by the caller if needed
            UserId = submission.UserId,
            SubmissionTime = submission.SubmissionTime
        };
        
        return dto;
    }
}
