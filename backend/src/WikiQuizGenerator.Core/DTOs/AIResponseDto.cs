namespace WikiQuizGenerator.Core.DTOs;

public class AIResponseDto
{
    public string ResponseTopic { get; set; }
    public string TopicUrl { get; set; }

    public IList<QuestionDto> Questions { get; set; }

    public AIResponseDto() // this might not be needed
    {
        Questions = new List<QuestionDto>();
    }
}
