using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.DomainObjects;
using CosmosUser = WikiQuizGenerator.Data.Cosmos.Models.User;

namespace WikiQuizGenerator.Data.Cosmos.Repositories;

public class CosmosUserRepository : IUserRepository
{
    private readonly CosmosDbContext _context;
    private readonly ILogger<CosmosUserRepository> _logger;

    public CosmosUserRepository(CosmosDbContext context, ILogger<CosmosUserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken)
    {
        try
        {
            var query = _context.UserContainer
                .GetItemLinqQueryable<CosmosUser>()
                .Where(u => u.RefreshToken == refreshToken)
                .ToFeedIterator();

            var results = await query.ReadNextAsync();
            var cosmosUser = results.FirstOrDefault();
            
            return cosmosUser != null ? MapFromCosmos(cosmosUser) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by refresh token");
            return null;
        }
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        try
        {
            var response = await _context.UserContainer.ReadItemAsync<CosmosUser>(
                userId.ToString(), 
                new PartitionKey(userId.ToString()));
            
            return MapFromCosmos(response.Resource);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by ID {UserId}", userId);
            return null;
        }
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        try
        {
            var query = _context.UserContainer
                .GetItemLinqQueryable<CosmosUser>()
                .Where(u => u.Email == email)
                .ToFeedIterator();

            var results = await query.ReadNextAsync();
            var cosmosUser = results.FirstOrDefault();
            
            return cosmosUser != null ? MapFromCosmos(cosmosUser) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by email {Email}", email);
            return null;
        }
    }

    public async Task<User> CreateUserAsync(User user)
    {
        try
        {
            var cosmosUser = MapToCosmos(user);
            cosmosUser.PartitionKey = cosmosUser.Id;
            
            var response = await _context.UserContainer.CreateItemAsync(cosmosUser, new PartitionKey(cosmosUser.PartitionKey));
            return MapFromCosmos(response.Resource);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user {Email}", user.Email);
            throw;
        }
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        try
        {
            var cosmosUser = MapToCosmos(user);
            cosmosUser.PartitionKey = cosmosUser.Id;
            
            var response = await _context.UserContainer.ReplaceItemAsync(
                cosmosUser, 
                cosmosUser.Id, 
                new PartitionKey(cosmosUser.PartitionKey));
            
            return MapFromCosmos(response.Resource);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", user.Id);
            throw;
        }
    }

    public async Task<double> GetUserCost(Guid userId, int timePeriod = 7)
    {
        try
        {
            var user = await GetUserByIdAsync(userId);
            return user?.WeeklyCost ?? 0.0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user cost for {UserId}", userId);
            return 0.0;
        }
    }

    public async Task UpdateUserPointsAndLevelAsync(Guid userId, int pointsToAdd, int newLevel)
    {
        try
        {
            var user = await GetUserByIdAsync(userId);
            if (user != null)
            {
                user.TotalPoints += pointsToAdd;
                user.Level = newLevel;
                await UpdateUserAsync(user);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating points and level for user {UserId}", userId);
            throw;
        }
    }

    private static User MapFromCosmos(CosmosUser cosmosUser)
    {
        return new User
        {
            Id = Guid.Parse(cosmosUser.Id),
            UserName = cosmosUser.UserName,
            Email = cosmosUser.Email,
            FirstName = cosmosUser.FirstName,
            LastName = cosmosUser.LastName,
            RefreshToken = cosmosUser.RefreshToken,
            RefreshTokenExpiresAtUtc = cosmosUser.RefreshTokenExpiresAtUtc,
            isPremium = cosmosUser.IsPremium,
            TotalCost = cosmosUser.TotalCost,
            WeeklyCost = cosmosUser.WeeklyCost,
            TotalPoints = cosmosUser.TotalPoints,
            Level = cosmosUser.Level,
            EmailConfirmed = true
        };
    }

    private static CosmosUser MapToCosmos(User coreUser)
    {
        return new CosmosUser
        {
            Id = coreUser.Id.ToString(),
            UserName = coreUser.UserName,
            Email = coreUser.Email,
            FirstName = coreUser.FirstName,
            LastName = coreUser.LastName,
            RefreshToken = coreUser.RefreshToken,
            RefreshTokenExpiresAtUtc = coreUser.RefreshTokenExpiresAtUtc,
            IsPremium = coreUser.isPremium,
            TotalCost = coreUser.TotalCost,
            WeeklyCost = coreUser.WeeklyCost,
            TotalPoints = coreUser.TotalPoints,
            Level = coreUser.Level
        };
    }
}