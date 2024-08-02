namespace WikiQuizGenerator.Core.DTOs;

public class QuizDto
{
    public string Title { get; set; }
    public IList<AIResponseDto> AIResponses { get; set; }

    public QuizDto()
    {
        AIResponses = new List<AIResponseDto>();
    }
}