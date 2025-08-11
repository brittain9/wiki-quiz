using WikiQuizGenerator.Core.DomainObjects;
using WikiQuizGenerator.Core.DTOs;
namespace WikiQuizGenerator.Core.Mappers;

// This is the response for when the user requests detailed information about the quiz
public static class QuizResultMapper
{
    public static QuizResultDto ToDto(Submission submission, Quiz quiz)
    {
        return new QuizResultDto()
        {
            Quiz = quiz.ToDto(),
            Score = submission.Score,
            SubmissionTime = submission.SubmissionTime,
            UserId = submission.UserId,
            Answers = GetResultOptions(submission, quiz)
        };
    }

    private static List<ResultOptionDto> GetResultOptions(Submission submission, Quiz quiz)
    {
        var resultOptions = new List<ResultOptionDto>();

        for (int i = 0; i < quiz.Questions.Count; i++)
        {
            var question = quiz.Questions[i];
            var selectedAnswer = i < submission.Answers.Count ? submission.Answers[i] : -1;

            resultOptions.Add(new ResultOptionDto
            {
                QuestionId = i + 1,
                CorrectAnswerChoice = question.CorrectAnswerIndex,
                SelectedAnswerChoice = selectedAnswer
            });
        }

        return resultOptions;
    }
}