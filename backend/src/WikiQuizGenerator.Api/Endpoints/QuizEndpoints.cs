using WikiQuizGenerator.Core.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;

namespace WikiQuizGenerator.Api.Endpoints
{
    public static class QuizEndpoints
    {
        public static void MapQuizEndpoints(this WebApplication app)
        {
            app.MapGet("/basicquiz", async (IQuizGenerator quizGenerator, string topic, string language = "en", int numQuestions = 5, int extractLength = 1000) =>
            {
                var quiz = await quizGenerator.GeneratorBasicQuizAsync(topic, language, numQuestions, extractLength);

                if (quiz == null) return Results.NoContent();

                Console.WriteLine($"Generated basic quiz on {topic} using {quiz.QuestionResponses[0].TotalTokens} tokens.");

                return Results.Ok(quiz);
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
        }
    }
}
