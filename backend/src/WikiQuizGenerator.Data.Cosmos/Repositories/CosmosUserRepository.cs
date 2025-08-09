using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using WikiQuizGenerator.Core.Interfaces;
using CosmosUser = WikiQuizGenerator.Data.Cosmos.Models.User;
using CoreUser = WikiQuizGenerator.Core.Models.User;

namespace WikiQuizGenerator.Data.Cosmos.Repositories;

public class CosmosUserRepository : IUserRepository
{
    private readonly CosmosDbContext _context;

    public CosmosUserRepository(CosmosDbContext context)
    {
        _context = context;
    }

    public async Task<CoreUser?> GetUserByRefreshTokenAsync(string refreshToken)
    {
        var iterator = _context.UserContainer
            .GetItemLinqQueryable<CosmosUser>()
            .Where(u => u.RefreshToken == refreshToken)
            .ToFeedIterator();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            var user = response.FirstOrDefault();
            if (user != null)
            {
                return MapFromCosmos(user);
            }
        }

        return null;
    }

    public async Task<CoreUser?> GetUserByIdAsync(Guid userId)
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
    }

    public async Task<double> GetUserCost(Guid userId, int timePeriod = 7)
    {
        var user = await GetUserByIdAsync(userId);
        return user?.WeeklyCost ?? 0.0;
    }

    public async Task UpdateUserPointsAndLevelAsync(Guid userId, int pointsToAdd, int newLevel)
    {
        var cosmosUser = await _context.UserContainer.ReadItemAsync<CosmosUser>(
            userId.ToString(), 
            new PartitionKey(userId.ToString()));

        if (cosmosUser.Resource != null)
        {
            cosmosUser.Resource.TotalPoints += pointsToAdd;
            cosmosUser.Resource.Level = newLevel;

            await _context.UserContainer.ReplaceItemAsync(
                cosmosUser.Resource, 
                cosmosUser.Resource.Id, 
                new PartitionKey(cosmosUser.Resource.PartitionKey));
        }
    }

    private CoreUser MapFromCosmos(CosmosUser cosmos)
    {
        var user = CoreUser.Create(cosmos.Email, cosmos.FirstName, cosmos.LastName);
        user.Id = Guid.Parse(cosmos.Id);
        user.RefreshToken = cosmos.RefreshToken;
        user.RefreshTokenExpiresAtUtc = cosmos.RefreshTokenExpiresAtUtc;
        user.isPremium = cosmos.IsPremium;
        user.TotalCost = cosmos.TotalCost;
        user.WeeklyCost = cosmos.WeeklyCost;
        user.TotalPoints = cosmos.TotalPoints;
        user.Level = cosmos.Level;
        user.UserName = cosmos.UserName;
        return user;
    }

    private CosmosUser MapToCosmos(CoreUser core)
    {
        return new CosmosUser
        {
            Id = core.Id.ToString(),
            PartitionKey = core.Id.ToString(),
            FirstName = core.FirstName,
            LastName = core.LastName,
            Email = core.Email ?? string.Empty,
            UserName = core.UserName ?? string.Empty,
            RefreshToken = core.RefreshToken,
            RefreshTokenExpiresAtUtc = core.RefreshTokenExpiresAtUtc,
            IsPremium = core.isPremium,
            TotalCost = core.TotalCost,
            WeeklyCost = core.WeeklyCost,
            TotalPoints = core.TotalPoints,
            Level = core.Level
        };
    }
}