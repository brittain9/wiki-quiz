using Microsoft.OpenApi.Models;
using WikiQuizGenerator.Core;
using WikiQuizGenerator.LLM;

namespace WikiQuizGenerator.Api;

public static class AiServiceEndpoints
{
    public static void MapAiServiceEndpoints(this WebApplication app)
    {
        app.MapGet("/getAiServices", () =>
        {
            var availableServices = new List<string>();

            if (SemanticKernelServiceExtensions.IsOpenAiAvailable)
                availableServices.Add("OpenAI");

            if (SemanticKernelServiceExtensions.IsPerplexityAvailable)
                availableServices.Add("Perplexity");

            return Results.Ok(availableServices);
        })
        .WithName("GetAiServices")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Get the available AI services.";
            return operation;
        });
        
        app.MapGet("/getOpenAiModels", () =>
        {
            var availableModels = AiServiceManager.OpenAiModelNames.Values.ToList();
            return Results.Ok(availableModels);
        })
        .WithName("GetOpenAiModels")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Get the available OpenAi models.";
            return operation;
        });
        
        app.MapGet("/getPerplexityModels", () =>
        {
            var availableModels = AiServiceManager.PerplexityModelNames.Values.ToList();
            return Results.Ok(availableModels);
        })
        .WithName("GetPerplexityModels")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Get the available Perplexity models.";
            return operation;
        });
    }
}