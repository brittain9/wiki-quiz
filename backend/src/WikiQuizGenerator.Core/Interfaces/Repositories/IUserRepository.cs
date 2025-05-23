using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Core.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByRefreshTokenAsync(string refreshToken);
    Task<User?> GetUserByIdAsync(Guid userId);
    Task<double> GetUserCost(Guid userId, int timePeriod = 7);

}