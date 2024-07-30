namespace WikiQuizGenerator.Core.DTOs;

public class QuizDTO
{
    // public int Id { get; set; }
    public string Title { get; set; }
    public List<QuestionResponseDTO> QuestionResponses { get; set; }

    public QuizDTO()
    {
        QuestionResponses = new List<QuestionResponseDTO>();
    }
}