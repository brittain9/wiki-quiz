using Microsoft.AspNetCore.Mvc;
using WikiQuizGenerator.Core.DTOs;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Mappers;
using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Api.Endpoints;

public static class QuizEndpoints
{
    public static void MapQuizEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/quiz")
                       .WithTags("Quiz");

        group.MapGet("/basicquiz", HandleGetBasicQuiz)
             .WithName("GetBasicQuiz")
             .WithOpenApi(operation => new(operation)
             {
                 Summary = "Generate basic quiz from topic/language",
                 Description = "Creates quiz using Wikipedia content. Language codes: en, fr, zh, es. Extract length affects token usage and question quality."
             })
             .Produces<QuizDto>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status400BadRequest) // For validation errors caught by middleware
             .Produces(StatusCodes.Status204NoContent) // Or 404 if generator returns null
             .Produces(StatusCodes.Status500InternalServerError); // If middleware catches other exceptions

        group.MapPost("/submitquiz", HandleSubmitQuiz)
             .WithName("SubmitQuiz")
             .WithOpenApi(operation => new(operation)
             {
                 Summary = "Submit quiz answers",
                 Description = "Processes user answers and returns score"
             })
             .Produces<SubmissionDto>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status404NotFound) // If quizId doesn't exist
             .Produces(StatusCodes.Status400BadRequest) // For invalid submission data or validation errors
             .Produces(StatusCodes.Status500InternalServerError); // If middleware catches other exceptions
    }

    private static async Task<IResult> HandleGetBasicQuiz(
        // Inject services needed
        [FromServices] IQuizGenerator quizGenerator,
        // Define query parameters
        [FromQuery] string topic,
        [FromQuery] int aiService,
        [FromQuery] int model,
        [FromQuery] string language = "en",
        [FromQuery] int numQuestions = 5,
        [FromQuery] int numOptions = 4,
        [FromQuery] int extractLength = 1000)
    {
        // Validation errors will be caught by middleware if they throw ArgumentException
        ValidateBasicQuizParameters(numQuestions, numOptions, extractLength);

        var lang = LanguagesExtensions.GetLanguageFromCode(language);

        var quiz = await quizGenerator.GenerateBasicQuizAsync(topic, lang, aiService, model, numQuestions, numOptions, extractLength);

        if (quiz == null)
        {
            return Results.NotFound("Could not generate quiz content for the given topic.");
        }

        return TypedResults.Ok(QuizMapper.ToDto(quiz));
    }

    private static async Task<IResult> HandleSubmitQuiz(
        [FromServices] IQuizRepository quizRepository,
        SubmissionDto submissionDto)
    {
        if (submissionDto == null || submissionDto.QuestionAnswers == null)
        {
            return Results.BadRequest(new { message = "Invalid submission data." });
        }

        var quiz = await quizRepository.GetByIdAsync(submissionDto.QuizId);
        if (quiz is null)
        {
            return TypedResults.NotFound(new { message = $"Quiz with ID {submissionDto.QuizId} not found." });
        }

        var submission = SubmissionMapper.ToModel(submissionDto);
        submission.Quiz = quiz;
        submission.Score = CalculateScore(submissionDto, quiz);
        submission.SubmissionTime = DateTime.UtcNow;
        // TODO: Link submission to user if needed

        await quizRepository.AddSubmissionAsync(submission);

        var resultDto = SubmissionMapper.ToDto(submission);
        return TypedResults.Ok(resultDto);
    }

    private static void ValidateBasicQuizParameters(int numQuestions, int numOptions, int extractLength)
    {
        // Throws ArgumentException on failure, which should be handled by ErrorHandlerMiddleware
        if (numQuestions < 1 || numQuestions > 20)
            throw new ArgumentException("Number of questions must be between 1 and 20.");

        if (numOptions < 2 || numOptions > 5)
            throw new ArgumentException("Number of options must be between 2 and 5.");

        if (extractLength < 100 || extractLength > 50000)
            throw new ArgumentException("Extract length must be between 100 and 50000 characters.");
    }

    private static int CalculateScore(SubmissionDto submissionDto, Quiz quiz)
    {
        if (quiz?.AIResponses == null || submissionDto?.QuestionAnswers == null) return 0;
        var allQuestions = quiz.AIResponses.SelectMany(r => r.Questions ?? Enumerable.Empty<Question>()).ToList();
        int totalQuestions = allQuestions.Count;
        if (totalQuestions == 0) return 0;
        int correctAnswers = 0;
        foreach (var question in allQuestions)
        {
            var answer = submissionDto.QuestionAnswers.FirstOrDefault(a => a.QuestionId == question.Id);
            if (answer != null && answer.SelectedOptionNumber == question.CorrectOptionNumber)
            {
                correctAnswers++;
            }
        }
        return (int)Math.Round((double)correctAnswers / totalQuestions * 100);
    }
}