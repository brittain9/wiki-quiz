using WikiQuizGenerator.Core.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using WikiQuizGenerator.Core.Models;
using Serilog;

namespace WikiQuizGenerator.Api;

public static class QuizEndpoints
{
    public static void MapQuizEndpoints(this WebApplication app)
    {
        app.MapGet("/basicquiz", async (IQuizGenerator quizGenerator, string topic, string language = "en", int numQuestions = 5, int extractLength = 1000) =>
        {
            try
            {
                var quiz = await quizGenerator.GenerateBasicQuizAsync(topic, language, numQuestions, extractLength);
                Log.Information($"Generated basic quiz on {topic} using {quiz.QuestionResponses[0].TotalTokens} tokens.");
                return Results.Ok(quiz);
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

        // Should I add a post method that allows for the quiz to be sumbitted and checked?
        // If I want to display older quizes with the user's answer choice, I need the post method.
        // it would also allow me to expose the answer choice to the user by keeping it on the server side.
    }
}
