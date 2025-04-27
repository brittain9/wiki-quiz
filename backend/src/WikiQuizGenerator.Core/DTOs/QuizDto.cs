namespace WikiQuizGenerator.Core.DTOs;

// This sent to frontend when user requests a quiz
public class QuizDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public DateTime CreatedAt { get; set; }
    public IList<AIResponseDto> AIResponses { get; set; }
}

public class AIResponseDto
{
    public int Id { get; set; }
    // Wikipedia page
    public string ResponseTopic { get; set; }
    public string TopicUrl { get; set; }

    // Metadata; user doesn't need to see this, but I want to for now.
    public long ResponseTime { get; set; } // in milliseconds
    public int? InputTokenCount { get; set; }
    public int? OutputTokenCount { get; set; }
    public string? ModelName { get; set; }

    public IList<QuestionDto> Questions { get; set; }
}

public class QuestionDto
{
    public int Id { get; set; }
    public string Text { get; set; }
    public IList<string> Options { get; set; }
}