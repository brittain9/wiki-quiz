namespace WikiQuizGenerator.Core.DTOs;

public class AnswerValidationDto
{
    public int QuestionId { get; set; }
    public int SelectedOptionNumber { get; set; }
}

public class AnswerValidationResponseDto
{
    public bool IsCorrect { get; set; }
    public int CorrectOptionNumber { get; set; }
    public int PointsEarned { get; set; }
    public string? CorrectAnswerText { get; set; }
}
