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

        group.MapPost("/basicquiz", HandleGetBasicQuiz)
             .WithName("GenerateBasicQuiz")
             .WithOpenApi(operation => new(operation)
             {
                 Summary = "Generate basic quiz from topic/language",
                 Description = "Creates quiz using Wikipedia content. Language codes: en, fr, zh, es. Extract length affects token usage and question quality."
             })
             .Produces<QuizDto>(StatusCodes.Status201Created)
             .ProducesValidationProblem() // Handles validation errors (like ArgumentException from Validate)
             .ProducesProblem(StatusCodes.Status404NotFound) // If quiz generation fails to find content
             .ProducesProblem(StatusCodes.Status500InternalServerError); // General errors

        group.MapPost("/submitquiz", HandleSubmitQuiz)
             .WithName("SubmitQuiz")
             .WithOpenApi(operation => new(operation)
             {
                 Summary = "Submit quiz answers",
                 Description = "Processes user answers, creates a submission record, and returns score"
             })
             .Produces<SubmissionDto>(StatusCodes.Status201Created)
             .ProducesProblem(StatusCodes.Status404NotFound) // If quizId doesn't exist
             .ProducesProblem(StatusCodes.Status400BadRequest) // For invalid submission data
             .ProducesValidationProblem() // For potential future model validation
             .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> HandleGetBasicQuiz(
        [FromServices] IQuizGenerator quizGenerator,
        [FromQuery] string topic,
        [FromQuery] int aiService,
        [FromQuery] int model,
        [FromQuery] string language = "en",
        [FromQuery] int numQuestions = 5,
        [FromQuery] int numOptions = 4,
        [FromQuery] int extractLength = 1000)
    {
        // Validation happens here (throws ArgumentException on failure)
        ValidateBasicQuizParameters(numQuestions, numOptions, extractLength);

        var lang = LanguagesExtensions.GetLanguageFromCode(language);

        var quiz = await quizGenerator.GenerateBasicQuizAsync(topic, lang, aiService, model, numQuestions, numOptions, extractLength);

        if (quiz == null)
        {
            return TypedResults.NotFound("Could not generate quiz content for the given topic.");
        }

        var quizDto = QuizMapper.ToDto(quiz);

        return TypedResults.Created((string?)null, quizDto);
 
    }

    private static async Task<IResult> HandleSubmitQuiz(
        [FromServices] IQuizRepository quizRepository,
        SubmissionDto submissionDto)
    {
        if (submissionDto == null || submissionDto.QuestionAnswers == null || submissionDto.QuizId <= 0)
        {
            return TypedResults.BadRequest("Invalid submission data provided.");
        }

        var quiz = await quizRepository.GetByIdAsync(submissionDto.QuizId);
        if (quiz is null)
        {
            return TypedResults.NotFound($"Quiz with ID {submissionDto.QuizId} not found.");
        }

        var submission = SubmissionMapper.ToModel(submissionDto);
        submission.Quiz = quiz; // Link to the fetched quiz
        submission.Score = CalculateScore(submissionDto, quiz);
        submission.SubmissionTime = DateTime.UtcNow;

        // Assume AddSubmissionAsync populates submission.Id upon successful save
        await quizRepository.AddSubmissionAsync(submission);

        var resultDto = SubmissionMapper.ToDto(submission);

        var submissionUri = $"/api/quiz-submissions/{submission.Id}";
        return TypedResults.Created(submissionUri, resultDto);
    }


    private static void ValidateBasicQuizParameters(int numQuestions, int numOptions, int extractLength)
    {
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
        // Avoid division by zero if totalQuestions is somehow zero after checks
        return totalQuestions > 0 ? (int)Math.Round((double)correctAnswers / totalQuestions * 100) : 0;
    }
}