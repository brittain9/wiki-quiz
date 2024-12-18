using WikiQuizGenerator.Core.Interfaces;
using Microsoft.OpenApi.Models;
using Serilog;
using WikiQuizGenerator.Core.DTOs;
using WikiQuizGenerator.Core.Mappers;
using Microsoft.AspNetCore.SignalR;
using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Api;

public static class QuizEndpoints
{
    public static void MapQuizEndpoints(this WebApplication app)
    {
        app.MapGet("/basicquiz", HandleGetBasicQuiz)
           .WithName("GetBasicQuiz")
           .WithOpenApi(operation =>
           {
               operation.Summary = "Generate a basic quiz based on a given topic and language.";
               operation.Description =
                   "This endpoint generates a quiz only on the topic entered. The user can specify the topic that matches the language code entered (e.g., en, fr, zh, es). " +
                   "Extract Length is the amount of Wikipedia content used in the prompt and is directly correlated with prompt token usage and question variety and quality.";

               operation.Responses.Add("204", new OpenApiResponse
               {
                   Description = "No content. The Wikipedia page could not be found for the given topic."
               });

               return operation;
           });

        app.MapPost("/submitquiz", HandleSubmitQuiz)
           .WithName("SubmitQuiz")
           .WithOpenApi(operation =>
           {
               operation.Summary = "Submit answers for a quiz and get results.";
               operation.Description = "This endpoint allows users to submit their answers for a quiz and receive feedback on their performance.";
               return operation;
           });
    }

    private static async Task<IResult> HandleGetBasicQuiz(IQuizGenerator quizGenerator, string topic, int aiService, int model,
        string language = "en", int numQuestions = 5, int numOptions = 4, int extractLength = 1000)
    {
        Log.Verbose($"GET /basicquiz called with topic '{topic}' in '{language}' with {numQuestions} questions, {numOptions} options, and {extractLength} extract length.");

        var validationResult = ValidateBasicQuizParameters(numQuestions, numOptions, extractLength);
        if (validationResult is not null)
            return validationResult;

        try
        {
            var lang = LanguagesExtensions.GetLanguageFromCode(language);
            var quiz = await quizGenerator.GenerateBasicQuizAsync(topic, lang, aiService, model, numQuestions, numOptions, extractLength);

            Log.Verbose($"GET /basicquiz returning quiz with id '{quiz.Id}'.");
            return Results.Ok(QuizMapper.ToDto(quiz));
        }
        catch (LanguageException)
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
            Log.Error(ex, $"Failed generating basic quiz on {topic}.");
            return Results.NoContent();
        }
    }

    private static async Task<IResult> HandleSubmitQuiz(IQuizRepository quizRepository, SubmissionDto submissionDto)
    {
        Log.Verbose($"POST /submitquiz called on quiz Id '{submissionDto.QuizId}'.");

        try
        {
            var quiz = await quizRepository.GetByIdAsync(submissionDto.QuizId);
            if (quiz is null)
                return Results.NotFound($"Quiz with ID {submissionDto.QuizId} not found.");

            var submission = SubmissionMapper.ToModel(submissionDto);
            submission.Quiz = quiz;
            submission.Score = CalculateScore(submissionDto, quiz);

            await quizRepository.AddSubmissionAsync(submission);

            return Results.Ok(submission.ToDto());
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Error processing quiz submission for quiz ID: {submissionDto.QuizId}");
            return Results.BadRequest("An error occurred while processing your submission. Please try again.");
        }
    }

    private static IResult? ValidateBasicQuizParameters(int numQuestions, int numOptions, int extractLength)
    {
        if (numQuestions < 1 || numQuestions > 20)
            return Results.BadRequest("Number of questions must be between 1 and 20.");

        if (numOptions < 2 || numOptions > 5)
            return Results.BadRequest("Number of options must be between 2 and 5.");

        if (extractLength < 100 || extractLength > 50000)
            return Results.BadRequest("Extract length must be between 100 and 50000 characters.");

        return null;
    }

    private static int CalculateScore(SubmissionDto submissionDto, Quiz quiz)
    {
        int correctAnswers = 0;
        int totalQuestions = quiz.AIResponses.Sum(aiResponse => aiResponse.Questions.Count);

        foreach (var aiResponse in quiz.AIResponses)
        {
            foreach (var question in aiResponse.Questions)
            {
                var answer = submissionDto.QuestionAnswers.FirstOrDefault(a => a.QuestionId == question.Id);
                if (answer != null && answer.SelectedOptionNumber == question.CorrectOptionNumber)
                {
                    correctAnswers++;
                }
            }
        }

        return totalQuestions > 0 ? (int)Math.Round((double)correctAnswers / totalQuestions * 100) : 0;
    }
}
