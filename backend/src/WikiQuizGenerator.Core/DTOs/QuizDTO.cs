namespace WikiQuizGenerator.Core.DTOs;

public class QuizDto
{
    public string Title { get; set; }
    public DateTime CreatedAt { get; set; }
    public IList<AIResponseDto> AIResponses { get; set; }
}