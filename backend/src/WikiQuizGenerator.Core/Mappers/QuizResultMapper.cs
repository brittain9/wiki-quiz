using WikiQuizGenerator.Core.DTOs;
using WikiQuizGenerator.Core.Models;
using System;
using System.Linq;

namespace WikiQuizGenerator.Core.Mappers;

public static class QuizResultMapper
{
    public static QuizResultDto ToDto(this Quiz quiz, QuizSubmission submission)
    {
        if (quiz == null) throw new ArgumentNullException(nameof(quiz));
        if (submission == null) throw new ArgumentNullException(nameof(submission));

        var aiResponseResultDtos = new List<AIResponseResultDto>();
        int correctAnswers = 0;

        var answersDictionary = submission.Answers.ToDictionary(a => a.QuestionId, a => a.SelectedOptionNumber);

        foreach (var aiResponse in quiz.AIResponses ?? Enumerable.Empty<AIResponse>())
        {
            var questionResultDtos = new List<QuestionResultDto>();
            foreach (var question in aiResponse.Questions ?? Enumerable.Empty<Question>())
            {
                if (answersDictionary.TryGetValue(question.Id, out int userSelectedOptionNumber))
                {
                    var isCorrect = userSelectedOptionNumber == question.CorrectOptionNumber;
                    if (isCorrect) correctAnswers++;

                    questionResultDtos.Add(new QuestionResultDto
                    {
                        Id = question.Id,
                        Text = question.Text ?? "",
                        Options = new List<string> { question.Option1, question.Option2, question.Option3, question.Option4, question.Option5 }
                            .Where(o => !string.IsNullOrEmpty(o)).ToList(),
                        CorrectOptionNumber = question.CorrectOptionNumber,
                        UserSelectedOptionIndex = userSelectedOptionNumber
                    });
                }
                else
                {
                    // If no answer was provided, add the question without a user selection
                    questionResultDtos.Add(new QuestionResultDto
                    {
                        Id = question.Id,
                        Text = question.Text ?? "",
                        Options = new List<string> { question.Option1, question.Option2, question.Option3, question.Option4, question.Option5 }
                            .Where(o => !string.IsNullOrEmpty(o)).ToList(),
                        CorrectOptionNumber = question.CorrectOptionNumber,
                        UserSelectedOptionIndex = null
                    });
                }
            }

            aiResponseResultDtos.Add(new AIResponseResultDto
            {
                ResponseTopic = aiResponse.WikipediaPage?.Title ?? "Unknown Topic",
                Url = aiResponse.WikipediaPage?.Url ?? "Unknown Url",
                Questions = questionResultDtos
            });
        }

        return new QuizResultDto
        {
            Id = quiz.Id,
            Title = quiz.Title ?? "Untitled Quiz",
            CreatedAt = quiz.CreatedAt,
            AIResponses = aiResponseResultDtos,
            TotalQuestions = quiz.AIResponses?.Sum(ar => ar.Questions?.Count ?? 0) ?? 0,
            CorrectAnswers = correctAnswers
        };
    }
}
