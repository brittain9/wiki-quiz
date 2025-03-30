using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace WikiQuizGenerator.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly WikiQuizDbContext _applicationDbContext;

    public UserRepository(WikiQuizDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken)
    {
        var user = await _applicationDbContext.Users.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken);

        return user;
    }
}