using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using WikiQuizGenerator.Core.DTOs;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Mappers;
using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Api;

public static class QuizEndpoints
{
    public static void MapQuizEndpoints(this WebApplication app)
    {
        app.MapGet("/basicquiz", HandleGetBasicQuiz)
           .WithName("GetBasicQuiz")
           .WithOpenApi(operation => new()
           {
               Summary = "Generate basic quiz from topic/language",
               Description = "Creates quiz using Wikipedia content. Language codes: en, fr, zh, es. Extract length affects token usage and question quality.",
               Parameters = new List<OpenApiParameter>
               {
                   new() { Name = "topic", In = ParameterLocation.Query, Required = true },
                   new() { Name = "aiService", In = ParameterLocation.Query, Required = true },
                   new() { Name = "model", In = ParameterLocation.Query, Required = true },
                   new() { Name = "language", In = ParameterLocation.Query },
                   new() { Name = "numQuestions", In = ParameterLocation.Query },
                   new() { Name = "numOptions", In = ParameterLocation.Query },
                   new() { Name = "extractLength", In = ParameterLocation.Query }
               }
           })
           .Produces<QuizDto>(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status400BadRequest)
           .Produces(StatusCodes.Status204NoContent);

        app.MapPost("/submitquiz", HandleSubmitQuiz)
           .WithName("SubmitQuiz")
           .WithOpenApi(operation => new()
           {
               Summary = "Submit quiz answers",
               Description = "Processes user answers and returns score"
           })
           .Produces<SubmissionDto>(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status404NotFound);
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
        ValidateBasicQuizParameters(numQuestions, numOptions, extractLength);
        var lang = LanguagesExtensions.GetLanguageFromCode(language);
        var quiz = await quizGenerator.GenerateBasicQuizAsync(topic, lang, aiService, model, numQuestions, numOptions, extractLength);

        return TypedResults.Ok(QuizMapper.ToDto(quiz));
    }

    private static async Task<IResult> HandleSubmitQuiz(
        [FromServices] IQuizRepository quizRepository,
        [FromBody] SubmissionDto submissionDto)
    {
        var quiz = await quizRepository.GetByIdAsync(submissionDto.QuizId);
        if (quiz is null)
        {
            return TypedResults.NotFound();
        }

        var submission = SubmissionMapper.ToModel(submissionDto);
        submission.Quiz = quiz;
        submission.Score = CalculateScore(submissionDto, quiz);

        await quizRepository.AddSubmissionAsync(submission);
        return TypedResults.Ok(submission.ToDto());
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
