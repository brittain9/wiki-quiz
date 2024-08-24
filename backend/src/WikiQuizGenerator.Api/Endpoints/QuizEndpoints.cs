using WikiQuizGenerator.Core.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using Serilog;
using WikiQuizGenerator.Core.Models;
using WikiQuizGenerator.Core.DTOs;
using WikiQuizGenerator.Core.Mappers;
using WikiQuizGenerator.Core;
using Microsoft.AspNetCore.SignalR;
using WikiQuizGenerator.LLM;

namespace WikiQuizGenerator.Api;

public static class QuizEndpoints
{
    public static void MapQuizEndpoints(this WebApplication app)
    {

        // Returns our DTO
        app.MapGet("/basicquiz", async (IQuizGenerator quizGenerator, string topic, int aiService, int model, string language = "en", int numQuestions = 5, int numOptions = 4, int extractLength = 1000) =>
        {
            Log.Verbose($"GET /basicquiz called with topic '{topic}' in '{language}' with {numQuestions} questions, {numOptions} options, and {extractLength} extract length.");

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

                var quiz = await quizGenerator.GenerateBasicQuizAsync(topic, lang, aiService, model, numQuestions, numOptions, extractLength);

                Log.Verbose($"GET /basicquiz returning quiz with id '{quiz.Id}'.");
                return Results.Ok(QuizMapper.ToDto(quiz));
            }
            catch (LanguageException ex)
            {
                Log.Error($"Invalid language code: {language}");
                return Results.BadRequest($"Invalid language code: {language}");
            }
            catch (ArgumentException ex) when (ex.Message.Contains("Wikipedia page"))
            {
                Log.Error($"Wikipedia page not found for topic: {topic}");
                return Results.NotFound($"Wikipedia page not found for topic: {topic}");
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
            Log.Verbose($"POST /submitquiz called on quiz Id '{submissionDto.QuizId}'.");

            try
            {
                var quiz = await quizRepository.GetByIdAsync(submissionDto.QuizId);

                if (quiz == null)
                {
                    Log.Warning($"Quiz not found for ID: {submissionDto.QuizId}");
                    return Results.NotFound("Quiz not found");
                }

                var questions = quiz.AIResponses?
                    .SelectMany(ar => ar.Questions ?? Enumerable.Empty<Question>())
                    .ToList() ?? new List<Question>();

                if (submissionDto.QuestionAnswers.Count > questions.Count)
                {
                    Log.Warning($"Submission contains more answers than questions. Quiz ID: {submissionDto.QuizId}");
                    return Results.BadRequest($"Submission contains more answers ({submissionDto.QuestionAnswers.Count}) than questions in the quiz ({questions.Count})");
                }

                var submission = QuizSubmissionMapper.ToModel(submissionDto);
                submission.SubmissionTime = DateTime.UtcNow;

                // Calculate score
                int correctAnswers = 0;
                foreach (var answer in submission.Answers)
                {
                    var question = questions.FirstOrDefault(q => q.Id == answer.QuestionId);
                    if (question != null && question.CorrectOptionNumber == answer.SelectedOptionNumber)
                    {
                        correctAnswers++;
                    }
                }

                // Add submission to database
                await quizRepository.AddSubmissionAsync(submission);

                // Generate quiz result
                var result = QuizResultMapper.ToDto(quiz, submission);
                result.CorrectAnswers = correctAnswers;
                result.TotalQuestions = questions.Count;

                Log.Information($"Successfully processed submission for quiz ID: {submissionDto.QuizId}. Score: {correctAnswers}/{questions.Count}");
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error processing quiz submission for quiz ID: {submissionDto.QuizId}");
                return Results.BadRequest("An error occurred while processing your submission. Please try again.");
            }
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