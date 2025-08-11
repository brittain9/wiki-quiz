using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Data.Cosmos.Configuration;
using WikiQuizGenerator.Data.Cosmos.Repositories;
using WikiQuizGenerator.Data.Cosmos.Services;

namespace WikiQuizGenerator.Data.Cosmos;

public static class CosmosDataServiceExtensions
{
    public static IServiceCollection AddCosmosDataServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure options
        services.Configure<CosmosDbOptions>(options => configuration.GetSection(CosmosDbOptions.SectionName).Bind(options));

        // Register CosmosClient
        services.AddSingleton<CosmosClient>(serviceProvider =>
        {
            var cosmosOptions = configuration.GetSection(CosmosDbOptions.SectionName).Get<CosmosDbOptions>();
            if (cosmosOptions?.ConnectionString == null)
            {
                throw new InvalidOperationException("Cosmos DB connection string is not configured.");
            }

            // Configure Cosmos client options
            var clientOptions = new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
                    IgnoreNullValues = true
                }
            };

            return new CosmosClient(cosmosOptions.ConnectionString, clientOptions);
        });

        // Register CosmosDbContext
        services.AddScoped<CosmosDbContext>();

        // Register repositories - these replace the EF Core ones
        services.AddScoped<IQuizRepository, CosmosQuizRepository>();
        services.AddScoped<CosmosUserRepository>(); // Register the concrete class for dependency injection
        services.AddScoped<IUserRepository>(provider => provider.GetRequiredService<CosmosUserRepository>());
        
        // Register Cosmos-based Account Service
        services.AddScoped<IAccountService, CosmosAccountService>();
        
        // Note: IWikipediaPageRepository is no longer needed since we don't store Wikipedia pages

        return services;
    }
}