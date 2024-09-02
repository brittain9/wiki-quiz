using WikiQuizGenerator.Core.Models;
using WikiQuizGenerator.Core.DTOs;
namespace WikiQuizGenerator.Core.Mappers;

// This is the resoponse for when the the user requests detailed information about the quiz
public static class QuizResultMapper
{
    public static QuizResultDto ToDto(Submission submission)
    {
        return new QuizResultDto()
        {
            Quiz = submission.Quiz.ToDto(),
            Score = submission.Score,
            SubmissionTime = submission.SubmissionTime,
            Answers = GetResultOptions(submission)
        };
    }

    private static List<ResultOptionDto> GetResultOptions(Submission submission)
    {
        var resultOptions = new List<ResultOptionDto>();

        foreach (var aiResponse in submission.Quiz.AIResponses)
        {
            foreach (var question in aiResponse.Questions)
            {
                var selectedAnswer = submission.Answers.FirstOrDefault(a => a.QuestionId == question.Id);

                resultOptions.Add(new ResultOptionDto
                {
                    QuestionId = question.Id,
                    CorrectAnswerChoice = question.CorrectOptionNumber,
                    SelectedAnswerChoice = selectedAnswer?.SelectedOptionNumber ?? 0
                });
            }
        }

        return resultOptions;
    }
}