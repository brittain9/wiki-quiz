using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using WikiQuizGenerator.Data.Cosmos.Configuration;

namespace WikiQuizGenerator.Data.Cosmos;

public class CosmosDbContext
{
    private readonly CosmosClient _cosmosClient;
    private readonly CosmosDbOptions _options;
    
    public CosmosDbContext(CosmosClient cosmosClient, IOptions<CosmosDbOptions> options)
    {
        _cosmosClient = cosmosClient;
        _options = options.Value;
    }
    
    public Container QuizContainer => _cosmosClient.GetContainer(_options.DatabaseName, _options.QuizContainerName);
    public Container UserContainer => _cosmosClient.GetContainer(_options.DatabaseName, _options.UserContainerName);
    
    /// <summary>
    /// Creates the database and containers if they don't exist
    /// </summary>
    public async Task EnsureContainersExistAsync()
    {
        var database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(_options.DatabaseName);
        
        // Quiz container partitioned by CreatedBy (user ID) for optimal query patterns
        await database.Database.CreateContainerIfNotExistsAsync(
            _options.QuizContainerName, 
            "/partitionKey",
            throughput: 400); // Start with minimal throughput
        
        // User container partitioned by id
        await database.Database.CreateContainerIfNotExistsAsync(
            _options.UserContainerName, 
            "/partitionKey",
            throughput: 400);
    }
}