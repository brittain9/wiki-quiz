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

        app.MapPost("/submitquiz", async (IQuizRepository quizRepository, SubmissionDto submissionDto) =>
        {
            Log.Verbose($"POST /submitquiz called on quiz Id '{submissionDto.QuizId}'.");

            try
            {
                var submission = SubmissionMapper.ToModel(submissionDto);
                
                // Get the quiz associated with the submission
                var quiz = await quizRepository.GetByIdAsync(submissionDto.QuizId) 
                           ?? throw new Exception($"Quiz not found for ID: {submissionDto.QuizId}");
                
                var questions = quiz.AIResponses?
                    .SelectMany(ar => ar.Questions ?? Enumerable.Empty<Question>())
                    .ToList() ?? new List<Question>();

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
                submission.Score = correctAnswers / questions.Count * 100;      
                
                await quizRepository.AddSubmissionAsync(submission);
                
                // Generate response
                var result = submission.ToDto();
                
                Log.Information($"Successfully processed submission for quiz ID: {submissionDto.QuizId}");
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