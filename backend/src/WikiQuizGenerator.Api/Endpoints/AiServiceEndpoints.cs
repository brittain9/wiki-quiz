using WikiQuizGenerator.Core.Interfaces;

namespace WikiQuizGenerator.Api;

public static class AiServiceEndpoints
{
    public static void MapAiServiceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/ai")
                       .WithTags("AI");

        // Endpoint for getting available AI services
        group.MapGet("/services", (IAiServiceManager aiServiceManager)
            => Results.Ok(aiServiceManager.GetAvailableAiServices()))
        .WithName("GetAiServices")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Get the available AI services.";
            return operation;
        });

        // Endpoint for getting models based on AI service ID
        group.MapGet("/models", (IAiServiceManager aiServiceManager, int? aiServiceId)
            => Results.Ok(aiServiceManager.GetModels(aiServiceId)))
        .WithName("GetAiModels")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Get the available models based on the AI service ID.";
            return operation;
        });
    }
}