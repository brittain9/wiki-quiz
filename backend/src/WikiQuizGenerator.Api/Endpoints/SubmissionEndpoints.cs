using WikiQuizGenerator.Core.DTOs;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Mappers;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WikiQuizGenerator.Api;

public static class SubmissionEndpoints
{
    public static void MapSubmissionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/submission")
                       .WithTags("Submission")
                       .RequireAuthorization();

        group.MapGet("/quizsubmission/{id:int}", async (int id, IQuizRepository quizRepository, ClaimsPrincipal user) =>
        {
            return await GetUserQuizSubmissionById(id, quizRepository, user);
        });

        group.MapGet("/quizsubmission/recent", async (IQuizRepository quizRepository, ClaimsPrincipal user) =>
        {
            return await GetRecentSubmissions(quizRepository, user);
        });

        group.MapGet("/my-submissions", async (IQuizRepository quizRepository, ClaimsPrincipal user) =>
        {
            return await GetUserSubmissions(quizRepository, user);
        });
    }

    private static async Task<IResult> GetUserQuizSubmissionById(int id, IQuizRepository quizRepository, ClaimsPrincipal claimsPrincipal)
    {
        var userId = GetUserIdFromClaims(claimsPrincipal);
        if (userId == Guid.Empty)
        {
            return Results.Unauthorized();
        }

        var submission = await quizRepository.GetUserSubmissionByIdAsync(id, userId);
        if (submission == null) return Results.NotFound();

        return Results.Ok(QuizResultMapper.ToDto(submission));
    }

    private static async Task<IResult> GetRecentSubmissions(IQuizRepository quizRepository, ClaimsPrincipal claimsPrincipal)
    {
        // Get the current user's ID from claims
        var userId = GetUserIdFromClaims(claimsPrincipal);
        if (userId == Guid.Empty)
        {
            return Results.Unauthorized();
        }
        
        // Get submissions for the current user only
        var userSubmissions = await quizRepository.GetSubmissionsByUserIdAsync(userId);
        
        if (userSubmissions == null || !userSubmissions.Any())
        {
            return Results.Ok(Array.Empty<SubmissionResponseDto>());
        }
        
        // Take the most recent submissions first (they should already be ordered by date in the repository)
        var submissionDtos = userSubmissions
            .Take(10) // Limit to 10 most recent
            .Select(submission => submission.ToDto())
            .ToList();
            
        return Results.Ok(submissionDtos);
    }

    private static async Task<IResult> GetUserSubmissions(IQuizRepository quizRepository, ClaimsPrincipal claimsPrincipal)
    {
        var userId = GetUserIdFromClaims(claimsPrincipal);
        if (userId == Guid.Empty)
        {
            return Results.Unauthorized();
        }

        var userSubmissions = await quizRepository.GetSubmissionsByUserIdAsync(userId);

        if (userSubmissions == null || !userSubmissions.Any())
        {
            return Results.Ok(Array.Empty<SubmissionResponseDto>());
        }

        var submissionDtos = userSubmissions
            .Select(submission => submission.ToDto())
            .ToList();

        return Results.Ok(submissionDtos);
    }
    
    private static Guid GetUserIdFromClaims(ClaimsPrincipal claimsPrincipal)
    {
        var nameIdentifier = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(nameIdentifier, out var userId) ? userId : Guid.Empty;
    }
}