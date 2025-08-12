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

        // Quiz container partitioned by partitionKey (matches serverless; no provisioned throughput)
        await database.Database.CreateContainerIfNotExistsAsync(new ContainerProperties
        {
            Id = _options.QuizContainerName,
            PartitionKeyPath = "/partitionKey"
        });

        // User container partitioned by partitionKey with unique key on email (serverless; no throughput)
        await database.Database.CreateContainerIfNotExistsAsync(new ContainerProperties
        {
            Id = _options.UserContainerName,
            PartitionKeyPath = "/partitionKey",
            UniqueKeyPolicy = new UniqueKeyPolicy
            {
                UniqueKeys = { new UniqueKey { Paths = { "/email" } } }
            }
        });
    }
}