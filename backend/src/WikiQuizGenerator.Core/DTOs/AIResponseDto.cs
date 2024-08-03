namespace WikiQuizGenerator.Core.DTOs;

public class AIResponseDto
{
    // Wikipedia pagee
    public string ResponseTopic { get; set; }
    public string TopicUrl { get; set; }

    // Metadata; user doesn't need to see this, but I want to for now.
    public long ResponseTime { get; set; } // in milliseconds
    public int? PromptTokenUsage { get; set; }
    public int? CompletionTokenUsage { get; set; }
    public string? ModelName { get; set; }

    public IList<QuestionDto> Questions { get; set; }
}
