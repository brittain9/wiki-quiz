using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Data.Repositories;

namespace WikiQuizGenerator.Data;

public static class DataServiceExtensions
{
    public static IServiceCollection AddDataServices(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<WikiQuizDbContext>(options =>
            options.UseNpgsql(connectionString,
                npgsqlOptions => npgsqlOptions.UseNodaTime()));

        // Register repositories 
        services.AddScoped<IQuizRepository, QuizRepository>();
        services.AddScoped<IWikipediaPageRepository, WikipediaPageRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
