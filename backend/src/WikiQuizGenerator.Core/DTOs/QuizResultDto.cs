namespace WikiQuizGenerator.Core.DTOs;

public class QuizResultDto
{
        public QuizDto Quiz { get; set; }
        public List<ResultOptionDto> Answers { get; set; }
        public DateTime SubmissionTime { get; set; }
        public int Score { get; set; }
}

public class ResultOptionDto
{
        public int QuestionId { get; set; }
        public int CorrectAnswerChoice { get; set; }
        public int SelectedAnswerChoice { get; set; }
}