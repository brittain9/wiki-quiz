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
        });
        
        app.MapGet("quizsubmission/recent", async (IQuizRepository quizRepository) =>
        {
            var recentQuizzes = await quizRepository.GetRecentQuizSubmissionsAsync();
            List<SubmissionResponseDto> recentSubmissionDtos = new();

            foreach (Submission recentSubmission in recentQuizzes)
            {
                recentSubmissionDtos.Add(recentSubmission.ToDto());
            }
            
            return Results.Ok(recentSubmissionDtos);
        });
    }
}