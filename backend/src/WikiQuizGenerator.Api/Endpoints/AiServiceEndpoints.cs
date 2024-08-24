using Microsoft.OpenApi.Models;
using WikiQuizGenerator.Core;
using WikiQuizGenerator.LLM;

namespace WikiQuizGenerator.Api;

public static class AiServiceEndpoints
{
    public static void MapAiServiceEndpoints(this WebApplication app)
    {
        app.MapGet("/getAiServices", (AiServiceManager aiServiceManager) =>
        {
            var availableServices = new Dictionary<int, string>();

            if (AiServiceManager.IsOpenAiAvailable)
                availableServices.Add((int)AiService.OpenAi, "OpenAI");

            if (AiServiceManager.IsPerplexityAvailable)
                availableServices.Add((int)AiService.Perplexity, "Perplexity");

            return Results.Ok(availableServices);
        })
        .WithName("GetAiServices")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Get the available AI services.";
            return operation;
        });
        
        app.MapGet("/getModels", (int? aiServiceId) =>
        {
            if (!aiServiceId.HasValue)
            {
                return Results.BadRequest("AI Service ID is required");
            }
            
            switch ((AiService)aiServiceId.Value)
            {
                case AiService.OpenAi:
                    return Results.Ok(AiServiceManager.OpenAiModelNames
                        .ToDictionary(kvp => (int)kvp.Key, kvp => kvp.Value));
                case AiService.Perplexity:
                    return Results.Ok(AiServiceManager.PerplexityModelNames
                        .ToDictionary(kvp => (int)kvp.Key, kvp => kvp.Value));
                default:
                    return Results.NotFound();
            }
        })
        .WithName("GetOpenAiModels")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Get the available models based on the AI service ID.";
            return operation;
        });
    }
}