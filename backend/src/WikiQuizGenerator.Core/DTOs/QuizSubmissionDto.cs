namespace WikiQuizGenerator.Core.DTOs;

// This is received from frontend when user submits a quiz
public class QuizSubmissionDto
{
    public int QuizId { get; set; }
    public List<QuestionAnswerDto> QuestionAnswers { get; set; } = new List<QuestionAnswerDto>();
}

public class QuestionAnswerDto
{
    public int QuestionId { get; set; }
    public int SelectedOptionNumber { get; set; }
}