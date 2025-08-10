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

        // TODO: Make sure only the owner can access the submission
        group.MapGet("/quizsubmission/{id:int}", async (int id, IQuizRepository quizRepository, ClaimsPrincipal user) =>
        {
            return await GetUserQuizSubmissionById(id, quizRepository, user);
        });

        group.MapGet("/my-submissions", async (IQuizRepository quizRepository, ClaimsPrincipal user, [FromQuery] int page = 1, [FromQuery] int pageSize = 10) =>
        {
            return await GetUserSubmissionsPaginated(quizRepository, user, page, pageSize);
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

        // We need to get the quiz separately since Submission no longer has Quiz navigation property
        var quiz = await quizRepository.GetByIdAsync(submission.QuizId, CancellationToken.None);
        if (quiz == null) return Results.NotFound("Associated quiz not found.");

        return Results.Ok(QuizResultMapper.ToDto(submission, quiz));
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

    private static async Task<IResult> GetUserSubmissionsPaginated(IQuizRepository quizRepository, ClaimsPrincipal claimsPrincipal, int page, int pageSize)
    {
        var userId = GetUserIdFromClaims(claimsPrincipal);
        if (userId == Guid.Empty)
        {
            return Results.Unauthorized();
        }

        // Validate pagination parameters
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100; // Limit max page size

        var (userSubmissions, totalCount) = await quizRepository.GetSubmissionsByUserIdPaginatedAsync(userId, page, pageSize);

        var submissionDtos = userSubmissions
            .Select(submission => submission.ToDto())
            .ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var paginatedResponse = new PaginatedResponseDto<SubmissionResponseDto>
        {
            Items = submissionDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };

        return Results.Ok(paginatedResponse);
    }
    
    private static Guid GetUserIdFromClaims(ClaimsPrincipal claimsPrincipal)
    {
        var nameIdentifier = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(nameIdentifier, out var userId) ? userId : Guid.Empty;
    }
}
