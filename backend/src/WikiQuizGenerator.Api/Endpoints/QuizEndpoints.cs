using Microsoft.AspNetCore.Mvc;
using WikiQuizGenerator.Core.DTOs;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Mappers;
using WikiQuizGenerator.Core.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WikiQuizGenerator.Core.Utilities;

namespace WikiQuizGenerator.Api.Endpoints;

public static class QuizEndpoints
{
    public static double UserCostLimit { get; set; } = 0.25;
    public static void MapQuizEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/quiz")
                       .WithTags("Quiz");

        group.MapPost("/basicquiz", HandleGetBasicQuiz)
             .WithName("GenerateBasicQuiz")
             .RequireRateLimiting("QuizLimit")
             .WithOpenApi(operation => new(operation)
             {
                 Summary = "Generate basic quiz from topic/language",
                 Description = "Creates quiz using Wikipedia content. Language codes: en, fr, zh, es. Extract length affects token usage and question quality."
             })
             .Produces<QuizDto>(StatusCodes.Status201Created)
             .ProducesValidationProblem()
             .ProducesProblem(StatusCodes.Status404NotFound)
             .ProducesProblem(StatusCodes.Status403Forbidden)
             .ProducesProblem(StatusCodes.Status429TooManyRequests)
             .ProducesProblem(StatusCodes.Status504GatewayTimeout)
             .ProducesProblem(StatusCodes.Status500InternalServerError)
             .RequireAuthorization();

        group.MapPost("/submitquiz", HandleSubmitQuiz)
             .WithName("SubmitQuiz")
             .RequireRateLimiting("QuizLimit")
             .WithOpenApi(operation => new(operation)
             {
                 Summary = "Submit quiz answers",
                 Description = "Processes user answers, creates a submission record, and returns score"
             })
             .Produces<SubmissionDto>(StatusCodes.Status201Created)
             .ProducesProblem(StatusCodes.Status404NotFound) // If quizId doesn't exist
             .ProducesProblem(StatusCodes.Status400BadRequest) // For invalid submission data
             .ProducesProblem(StatusCodes.Status429TooManyRequests)
             .ProducesProblem(StatusCodes.Status504GatewayTimeout)
             .ProducesProblem(StatusCodes.Status500InternalServerError)
             .RequireAuthorization();

        group.MapPost("/validateanswer", HandleValidateAnswer)
             .WithName("ValidateAnswer")
             .WithOpenApi(operation => new(operation)
             {
                 Summary = "Validate a single answer",
                 Description = "Validates a single question answer and returns immediate feedback with points earned"
             })
             .Produces<AnswerValidationResponseDto>(StatusCodes.Status200OK)
             .ProducesProblem(StatusCodes.Status404NotFound) // If questionId doesn't exist
             .ProducesProblem(StatusCodes.Status400BadRequest) // For invalid answer data
             .ProducesProblem(StatusCodes.Status500InternalServerError)
             .RequireAuthorization();
    }

    private static async Task<IResult> HandleGetBasicQuiz(
        [FromServices] IQuizGenerator quizGenerator,
        [FromServices] IUserRepository userRepository,
        ClaimsPrincipal user,
        CancellationToken cancellationToken,
        [FromQuery] string topic,
        [FromQuery] string aiService,
        [FromQuery] string model,
        [FromQuery] string language = "en",
        [FromQuery] int numQuestions = 5,
        [FromQuery] int numOptions = 4,
        [FromQuery] int extractLength = 1000)
    {
        // Validation happens here (throws ArgumentException on failure)
        ValidateBasicQuizParameters(numQuestions, numOptions, extractLength, topic);

        // Get user ID from claims
        var userId = GetUserIdFromClaims(user);
        if (userId == Guid.Empty)
        {
            return TypedResults.Unauthorized();
        }

        // Check if user is within cost limits
        var costCheckResult = await CheckUserCostLimitAsync(userId, userRepository, cancellationToken);
        if (costCheckResult != null)
        {
            return costCheckResult; // Return the error if user exceeded limit
        }

        var lang = LanguagesUtils.GetLanguageFromCode(language);

        var quiz = await quizGenerator.GenerateQuizAsync(
            topic,
            lang,
            aiService,
            model,
            numQuestions,
            numOptions,
            extractLength,
            userId,
            cancellationToken);

        if (quiz == null)
        {
            return TypedResults.NotFound("Could not generate quiz content for the given topic.");
        }

        var quizDto = QuizMapper.ToDto(quiz);

        return TypedResults.Created((string?)null, quizDto);
    }

    private static async Task<IResult> HandleSubmitQuiz(
        SubmissionDto submissionDto, 
        IQuizRepository quizRepository, 
        IUserRepository userRepository,
        IPointsService pointsService,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        if (submissionDto == null || submissionDto.QuestionAnswers == null || submissionDto.QuizId <= 0)
        {
            return TypedResults.BadRequest("Invalid submission data provided.");
        }

        var quiz = await quizRepository.GetByIdAsync(submissionDto.QuizId, cancellationToken);
        if (quiz is null)
        {
            return TypedResults.NotFound($"Quiz with ID {submissionDto.QuizId} not found.");
        }

        var userId = GetUserIdFromClaims(claimsPrincipal);
        if (userId == Guid.Empty)
        {
            return TypedResults.Unauthorized();
        }

        var existingSubmission = await quizRepository.GetUserSubmissionByQuizIdAsync(submissionDto.QuizId, userId, cancellationToken);
        if (existingSubmission != null)
        {
            return TypedResults.Conflict("You have already submitted this quiz. Only one submission per quiz is allowed.");
        }

        var allQuestions = quiz.Questions.ToList();
        var (scorePercent, pointsEarned) = pointsService.CalculateScoreAndPoints(allQuestions, submissionDto.QuestionAnswers);
        
        // Get current user to calculate new level
        var user = await userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            return TypedResults.NotFound("User not found.");
        }
        
        var newTotalPoints = user.TotalPoints + pointsEarned;
        var newLevel = pointsService.CalculateLevel(newTotalPoints);

        var submission = SubmissionMapper.ToModel(submissionDto);
        submission.UserId = userId;
        submission.Score = scorePercent;
        submission.PointsEarned = pointsEarned;
        submission.SubmissionTime = DateTime.UtcNow;

        // Save submission and update user points/level
        await quizRepository.AddSubmissionAsync(submissionDto.QuizId, submission, cancellationToken);
        await userRepository.UpdateUserPointsAndLevelAsync(userId, pointsEarned, newLevel);

        var resultDto = SubmissionMapper.ToDto(submission);

        return TypedResults.Created((string?)null, resultDto);
    }

    private static async Task<IResult> HandleValidateAnswer(
        AnswerValidationDto answerValidation,
        IQuizRepository quizRepository,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        if (answerValidation == null || answerValidation.QuestionId <= 0)
        {
            return TypedResults.BadRequest("Invalid answer validation data provided.");
        }

        var userId = GetUserIdFromClaims(claimsPrincipal);
        if (userId == Guid.Empty)
        {
            return TypedResults.Unauthorized();
        }

        // This endpoint requires a way to locate a question; not supported with current domain (no question IDs).
        return TypedResults.BadRequest("Per-question validation is not supported in the current model.");

        // Unreachable with current model constraints; kept here for future support
    }

    

    private static Guid GetUserIdFromClaims(ClaimsPrincipal claimsPrincipal)
    {
        var nameIdentifier = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(nameIdentifier, out var userId) ? userId : Guid.Empty;
    }
    private static async Task<IResult> CheckUserCostLimitAsync(Guid userId, IUserRepository userRepository, CancellationToken cancellationToken)
    {
        // Get the user to check premium status
        var user = await userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            return TypedResults.NotFound("User not found.");
        }

        // Premium users have unlimited usage
        if (user.IsPremium)
        {
            return null; // No limit applied, allow request to proceed
        }

        // Check the user's current cost for the past week
        var currentCost = await userRepository.GetUserCost(userId);

        // If user exceeded limit, return an error
        if (currentCost >= UserCostLimit)
        {
            return TypedResults.Problem(
                title: "Usage Limit Exceeded",
                detail: $"You have reached your weekly AI usage limit of ${UserCostLimit:F2}. Consider upgrading to premium for unlimited access.",
                statusCode: StatusCodes.Status403Forbidden
            );
        }

        return null; // User is under limit, allow request to proceed
    }

    private static void ValidateBasicQuizParameters(int numQuestions, int numOptions, int extractLength, string topic)
    {
        if (numQuestions < 1 || numQuestions > 20)
            throw new ArgumentException("Number of questions must be between 1 and 20.");

        if (numOptions < 2 || numOptions > 5)
            throw new ArgumentException("Number of options must be between 2 and 5.");

        if (extractLength < 100 || extractLength > 50000)
            throw new ArgumentException("Extract length must be between 100 and 50000 characters.");

        // Validate topic is not empty and not too long (to prevent token abuse)
        if (string.IsNullOrWhiteSpace(topic))
            throw new ArgumentException("Topic cannot be empty.");

        if (topic.Length > 500)
            throw new ArgumentException("Topic length must be less than 500 characters.");

        // TODO: test this
        // Basic sanitization - remove potentially problematic characters
        if (topic.IndexOfAny(new[] { '<', '>', '{', '}', '[', ']', '`' }) >= 0)
            throw new ArgumentException("Topic contains invalid characters.");
    }

}
