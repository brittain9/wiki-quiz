using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WikiQuizGenerator.Core.DTOs;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Mappers;
using WikiQuizGenerator.Core.Services;

namespace WikiQuizGenerator.Api;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/user")
                       .WithTags("User")
                       .RequireAuthorization();

        // Quiz submissions (history/detail)
        group.MapGet("/submissions/{quizId:int}", async (int quizId, IQuizRepository quizRepository, ClaimsPrincipal user) =>
        {
            return await GetUserQuizSubmissionByQuizId(quizId, quizRepository, user);
        });

        group.MapGet("/submissions", async (
            IQuizRepository quizRepository,
            ClaimsPrincipal user,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10) =>
        {
            return await GetUserSubmissionsPaginated(quizRepository, user, page, pageSize);
        });

        // Usage/cost info
        group.MapGet("/usage", async (IUserRepository userRepository, ClaimsPrincipal user) =>
        {
            return await GetUserUsage(userRepository, user);
        });

        // Points/level stats
        group.MapGet("/stats", async (IUserRepository userRepository, IPointsService pointsService, ClaimsPrincipal user) =>
        {
            return await GetUserStats(userRepository, pointsService, user);
        });

        // Clear all submitted quizzes for current user
        group.MapDelete("/submissions", async (IQuizRepository quizRepository, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = GetUserIdFromClaims(user);
            if (userId == Guid.Empty) return Results.Unauthorized();

            var count = await quizRepository.DeleteAllSubmittedQuizzesForUserAsync(userId, ct);
            return Results.Ok(new { deleted = count });
        });
    }

    private static async Task<IResult> GetUserQuizSubmissionByQuizId(int quizId, IQuizRepository quizRepository, ClaimsPrincipal claimsPrincipal)
    {
        var userId = GetUserIdFromClaims(claimsPrincipal);
        if (userId == Guid.Empty)
        {
            return Results.Unauthorized();
        }

        var submission = await quizRepository.GetUserSubmissionByQuizIdAsync(quizId, userId, CancellationToken.None);
        if (submission == null) return Results.NotFound();

        var quiz = await quizRepository.GetByIdForUserAsync(quizId, userId, CancellationToken.None);
        if (quiz == null) return Results.NotFound("Associated quiz not found.");

        return Results.Ok(QuizResultMapper.ToDto(submission, quiz));
    }

    private static async Task<IResult> GetUserSubmissionsPaginated(IQuizRepository quizRepository, ClaimsPrincipal claimsPrincipal, int page, int pageSize)
    {
        var userId = GetUserIdFromClaims(claimsPrincipal);
        if (userId == Guid.Empty)
        {
            return Results.Unauthorized();
        }

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var (userSubmissions, totalCount) = await quizRepository.GetSubmissionsByUserIdPaginatedAsync(userId, page, pageSize);

        // Ensure newest first ordering
        var submissionDtos = userSubmissions
            .OrderByDescending(s => s.SubmissionTime)
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

    private static async Task<IResult> GetUserUsage(IUserRepository userRepository, ClaimsPrincipal claimsPrincipal)
    {
        var userId = GetUserIdFromClaims(claimsPrincipal);
        if (userId == Guid.Empty)
        {
            return Results.Unauthorized();
        }

        var user = await userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            return Results.NotFound("User not found.");
        }

        var cost = await userRepository.GetUserCost(userId);

        var usage = new UserUsageDto
        {
            UserId = userId,
            IsPremium = user.IsPremium,
            CurrentCost = cost,
            WeeklyCostLimit = Api.Endpoints.QuizEndpoints.UserCostLimit,
            Remaining = Math.Max(0, Api.Endpoints.QuizEndpoints.UserCostLimit - cost),
            PeriodDays = 7
        };

        return Results.Ok(usage);
    }

    private static async Task<IResult> GetUserStats(IUserRepository userRepository, IPointsService pointsService, ClaimsPrincipal claimsPrincipal)
    {
        var userId = GetUserIdFromClaims(claimsPrincipal);
        if (userId == Guid.Empty)
        {
            return Results.Unauthorized();
        }

        var user = await userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            return Results.NotFound("User not found.");
        }

        var nextLevel = user.Level + 1;
        var nextLevelRequirement = pointsService.GetPointsRequiredForNextLevel(user.Level);
        var pointsToNextLevel = Math.Max(0, nextLevelRequirement - user.TotalPoints);

        var dto = new UserStatsDto
        {
            UserId = user.Id,
            TotalPoints = user.TotalPoints,
            Level = user.Level,
            NextLevel = nextLevel,
            PointsRequiredForNextLevel = nextLevelRequirement,
            PointsToNextLevel = pointsToNextLevel
        };

        return Results.Ok(dto);
    }

    private static Guid GetUserIdFromClaims(ClaimsPrincipal claimsPrincipal)
    {
        var nameIdentifier = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(nameIdentifier, out var userId) ? userId : Guid.Empty;
    }
}


