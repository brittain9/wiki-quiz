using System.Security.Claims;
using WikiQuizGenerator.Core.Interfaces;

namespace WikiQuizGenerator.Api;

public static class AiEndpoints
{
    public static void MapAiEndpoints(this WebApplication app)
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
        // TODO: open ai has an endpoint for this /v1/models
        group.MapGet("/models", (IAiServiceManager aiServiceManager, string aiServiceId)
            => Results.Ok(aiServiceManager.GetModels(aiServiceId)))
        .WithName("GetAiModels")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Get the available models based on the AI service ID.";
            return operation;
        });

        // Endpoint for getting the current user's cost
        group.MapGet("/user-cost", async (IUserRepository userRepository, ClaimsPrincipal user, int timePeriod = 7) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Results.Unauthorized();
            }

            var totalCost = await userRepository.GetUserCost(parsedUserId, timePeriod);
            return Results.Ok(new { TotalCost = totalCost });
        })
        .WithName("GetUserCost") 
        .RequireAuthorization()
        .WithOpenApi(operation =>
        {
            operation.Summary = "Get the current user's cost.";
            operation.Description = "Returns the total cost for the authenticated user's AI usage over the specified time period.";
            return operation;
        });
    }
}