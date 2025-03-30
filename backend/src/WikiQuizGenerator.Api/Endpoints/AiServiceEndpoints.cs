using WikiQuizGenerator.Core.Interfaces;

namespace WikiQuizGenerator.Api;

public static class AiServiceEndpoints
{
    public static void MapAiServiceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/ai")
                       .WithTags("AI");

        // Endpoint for getting available AI services
        group.MapGet("/getAiServices", (IAiServiceManager aiServiceManager)
            => Results.Ok(aiServiceManager.GetAvailableAiServices()))
        .WithName("GetAiServices")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Get the available AI services.";
            return operation;
        });

        // Endpoint for getting models based on AI service ID
        group.MapGet("/getModels", (IAiServiceManager aiServiceManager, int? aiServiceId)
            => Results.Ok(aiServiceManager.GetModels(aiServiceId)))
        .WithName("GetModels")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Get the available models based on the AI service ID.";
            return operation;
        });
    }
}