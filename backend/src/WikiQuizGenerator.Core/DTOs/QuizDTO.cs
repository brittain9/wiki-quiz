namespace WikiQuizGenerator.Core.DTOs;

public class QuizDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public DateTime CreatedAt { get; set; }
    public IList<AIResponseDto> AIResponses { get; set; }
}