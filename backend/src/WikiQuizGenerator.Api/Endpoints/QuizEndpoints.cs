using WikiQuizGenerator.Core.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using Serilog;
using WikiQuizGenerator.Core.Models;
using WikiQuizGenerator.Core.DTOs;
using WikiQuizGenerator.Core.Mappers;

namespace WikiQuizGenerator.Api;

public static class QuizEndpoints
{
    public static void MapQuizEndpoints(this WebApplication app)
    {
        app.MapGet("/basicquiz", async(IQuizGenerator quizGenerator, string topic, string language = "en", int numQuestions = 5, int numOptions = 4, int extractLength = 1000) =>
        {
            try
            {
                var quiz = await quizGenerator.GenerateBasicQuizAsync(topic, language, numQuestions, numOptions, extractLength);
                Log.Information($"Generated basic quiz on {topic}" ); // TODO: Add token usage to logs again
                return Results.Ok(QuizMapper.ToDto(quiz));
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed generating basic quiz on {topic}.");
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

        // Add post method and expose DTOs.
    }
}
