using Microsoft.EntityFrameworkCore;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly WikiQuizDbContext _context;

    public UserRepository(WikiQuizDbContext applicationDbContext)
    {
        _context = applicationDbContext;
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public async Task<double> GetUserCost(Guid userId, int timePeriod = 7)
    {
        var time = DateTime.UtcNow.AddDays(-timePeriod);

        var aiResponsesList = await _context.Quizzes
            .Where(quiz => quiz.CreatedBy == userId && quiz.CreatedAt >= time)
            // Navigate through Quiz to get AIResponses
            .SelectMany(quiz => quiz.AIResponses)
            .ToListAsync();

        double totalCost = 0.0;
        foreach (var response in aiResponsesList)
        {
            var modelConfig = await _context.ModelConfigs
                .FirstOrDefaultAsync(m => m.Id == response.ModelConfigId);

            if (response.InputTokenCount == null || response.OutputTokenCount == null || modelConfig == null ||
                modelConfig.CostPer1MInputTokens == null || modelConfig.CostPer1MOutputTokens == null)
            {
                return 0.0;
            }

            double inputCost = (response.InputTokenCount.Value / 1_000_000.0) * modelConfig.CostPer1MInputTokens;
            double outputCost = (response.OutputTokenCount.Value / 1_000_000.0) * modelConfig.CostPer1MOutputTokens;

            totalCost += (inputCost + outputCost);
        }

        return totalCost;
    }
}