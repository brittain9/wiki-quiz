using WikiQuizGenerator.Core.DomainObjects;

namespace WikiQuizGenerator.Core.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByRefreshTokenAsync(string refreshToken);
    Task<User?> GetUserByIdAsync(Guid userId);
    Task<double> GetUserCost(Guid userId, int timePeriod = 7);
    Task UpdateUserPointsAndLevelAsync(Guid userId, int pointsToAdd, int newLevel);

}
