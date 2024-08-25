using WikiQuizGenerator.Core.Interfaces;

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
                return Results.Ok(submission);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });
        
        app.MapGet("quizsubmission/recent", async (IQuizRepository quizRepository) =>
        {
            var recentQuizzes = await quizRepository.GetRecentQuizSubmissionsAsync();
            return Results.Ok(recentQuizzes);
        });
    }
}