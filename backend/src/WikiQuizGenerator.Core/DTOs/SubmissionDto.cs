using WikiQuizGenerator.Core.Models;
namespace WikiQuizGenerator.Core.DTOs;

// This is received from frontend when user submits a quiz
public class SubmissionDto
{
    public int QuizId { get; set; }
    public List<QuestionAnswerDto> QuestionAnswers { get; set; } = new List<QuestionAnswerDto>();
}

public class QuestionAnswerDto
{
    public int QuestionId { get; set; }
    public int SelectedOptionNumber { get; set; }
}

// This is sent back after the submission is received.
public class SubmissionResponseDto
{
    public int Id { get; set; } // id of the submission
    public string Title { get; set; }
    public int Score { get; set; }

    public DateTime SubmissionTime { get; set; }
}