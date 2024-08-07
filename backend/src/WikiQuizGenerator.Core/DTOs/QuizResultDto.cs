namespace WikiQuizGenerator.Core.DTOs;

// This is sent to frontend after checking the user's submission
public class QuizResultDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<AIResponseResultDto> AIResponses { get; set; }

    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
}

public class AIResponseResultDto
{
    public string ResponseTopic { get; set; }
    public string Url { get; set; }
    public List<QuestionResultDto> Questions { get; set; }
}

public class QuestionResultDto
{
    public int Id { get; set; }
    public string Text { get; set; }
    public List<string> Options { get; set; }
    public int CorrectOptionNumber { get; set; }
    public int? UserSelectedOption { get; set; }
}