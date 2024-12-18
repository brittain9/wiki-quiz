using System;
using WikiQuizGenerator.Core.DTOs;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Mappers;
using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Api;

public static class SubmissionEndpoints
{
    public static void MapSubmissionEndpoints(this WebApplication app)
    {
        app.MapGet("quizsubmission/{id}", async (int id, IQuizRepository quizRepository) =>
        {
            return await GetQuizSubmissionById(id, quizRepository);
        });

        app.MapGet("quizsubmission/recent", async (IQuizRepository quizRepository, int num = 10) =>
        {
            return await GetRecentQuizSubmissions(num, quizRepository);
        });
    }

    private static async Task<IResult> GetQuizSubmissionById(int id, IQuizRepository quizRepository)
    {
        try
        {
            var submission = await quizRepository.GetSubmissionByIdAsync(id);
            if (submission == null) return Results.NotFound();

            return Results.Ok(QuizResultMapper.ToDto(submission));
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }

    private static async Task<IResult> GetRecentQuizSubmissions(int num, IQuizRepository quizRepository)
    {
        try
        {
            var recentQuizzes = await quizRepository.GetRecentQuizSubmissionsAsync(num);

            if (recentQuizzes == null || !recentQuizzes.Any())
            {
                return Results.Ok(Array.Empty<SubmissionDto>());
            }

            var recentSubmissionDtos = recentQuizzes
                .Select(submission => submission.ToDto())
                .ToList();

            return Results.Ok(recentSubmissionDtos);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new
            {
                Error = ex.Message
            });
        }
    }


}
