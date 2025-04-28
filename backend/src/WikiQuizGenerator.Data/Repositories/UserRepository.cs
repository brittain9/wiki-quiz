using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using Microsoft.VisualBasic;

namespace WikiQuizGenerator.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly WikiQuizDbContext _applicationDbContext;

    public UserRepository(WikiQuizDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _applicationDbContext.Users.FindAsync(userId);
    }

    public async Task<double> GetUserCost(Guid userId, int timePeriod = 7)
    {
        var time = DateTime.UtcNow.AddDays(-timePeriod);

        var aiResponsesList = await _applicationDbContext.QuizSubmissions
            .Where(submission => submission.UserId == userId && submission.SubmissionTime >= time)
            // Navigate through Quiz to get AIResponses
            .SelectMany(submission => submission.Quiz.AIResponses)
            .Include(response => response.ModelConfig)
            .ToListAsync();

        var cost = aiResponsesList.Sum(response => response.CalculateCost());

        return cost;
    }
}