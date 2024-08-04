using WikiQuizGenerator.Core.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using Serilog;
using WikiQuizGenerator.Core.Models;
using WikiQuizGenerator.Core.DTOs;
using WikiQuizGenerator.Core.Mappers;
using WikiQuizGenerator.Core;

namespace WikiQuizGenerator.Api;

public static class QuizEndpoints
{
    public static void MapQuizEndpoints(this WebApplication app)
    {

        // Returns our DTO
        app.MapGet("/basicquiz", async(IQuizGenerator quizGenerator, string topic, string language = "en", int numQuestions = 5, int numOptions = 4, int extractLength = 1000) =>
        {
            // Validate input parameters
            if (numQuestions < 1 || numQuestions > 20)
            {
                return Results.BadRequest("Number of questions must be between 1 and 20.");
            }

            if (numOptions < 2 || numOptions > 5)
            {
                return Results.BadRequest("Number of options must be between 2 and 5.");
            }

            if (extractLength < 100 || extractLength > 50000)
            {
                return Results.BadRequest("Extract length must be between 100 and 50000 characters.");
            }

            try
            {
                Languages lang = LanguagesExtensions.GetLanguageFromCode(language); // this will throw error if language is not found

                var quiz = await quizGenerator.GenerateBasicQuizAsync(topic, lang, numQuestions, numOptions, extractLength);

                Log.Information($"Generated basic quiz on {topic}");
                return Results.Ok(QuizMapper.ToDto(quiz));
            }
            catch (ArgumentException ex) when (ex.Message.Contains("language"))
            {
                Log.Error($"Invalid language code: {language}");
                return Results.BadRequest($"Invalid language code: {language}");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed generating basic quiz on {topic}.");
                return Results.NoContent();
            }
        })
       .WithName("GetBasicQuiz")
       .WithOpenApi(operation =>
       {
           operation.Summary = "Generate a basic quiz based on a given topic and language.";
           operation.Description = "This endpoint generates a quiz only on the topic entered. The user can specify the topic that matches the langauge code entered (eg: en, fr, zh, es). " +
           "Extract Length is the amount of Wikipedia content used in the prompt and is directly correleated with prompt token usage and question variety and quality.";

           operation.Responses.Add("204", new OpenApiResponse
           {
               Description = "No content. The Wikipedia page could not be found for the given topic."
           });

           return operation;
       });


        app.MapPost("/submitquiz", async (IQuizRepository quizRepository, QuizSubmissionDto submissionDto) =>
        {
            var quiz = await quizRepository.GetByIdAsync(submissionDto.QuizId);

            if (quiz == null)
            {
                return Results.NotFound("Quiz not found");
            }

            var questions = quiz.AIResponses.SelectMany(ar => ar.Questions).ToList();

            if (submissionDto.UserAnswers.Count != questions.Count)
            {
                return Results.BadRequest("Number of answers doesn't match number of questions");
            }

            var result = new QuizResultDto
            {
                QuizId = quiz.Id,
                Questions = new List<QuestionResultDto>(),
                TotalQuestions = questions.Count
            };

            for (int i = 0; i < questions.Count; i++)
            {
                var question = questions[i];
                var userAnswer = submissionDto.UserAnswers[i];
                var isCorrect = userAnswer == question.CorrectOptionNumber;

                result.Questions.Add(new QuestionResultDto
                {
                    QuestionId = question.Id,
                    UserAnswer = userAnswer,
                    CorrectAnswer = question.CorrectOptionNumber,
                    IsCorrect = isCorrect
                });

                if (isCorrect)
                {
                    result.Score++;
                }
            }

            // Save the submission
            var submission = new QuizSubmission
            {
                QuizId = quiz.Id,
                Answers = submissionDto.UserAnswers,
                SubmissionTime = DateTime.UtcNow,
                Score = result.Score
            };

            await quizRepository.AddSubmissionAsync(submission);

            return Results.Ok(result);
        })
        .WithName("SubmitQuiz")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Submit answers for a quiz and get results.";
            operation.Description = "This endpoint allows users to submit their answers for a quiz and receive feedback on their performance.";

            return operation;
        });
    }
}