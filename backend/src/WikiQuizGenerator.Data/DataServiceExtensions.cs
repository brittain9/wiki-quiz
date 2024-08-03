using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Data.Repositories;

namespace WikiQuizGenerator.Data;

public static class DataServiceExtensions
{
    public static IServiceCollection AddDataServices(this IServiceCollection services)
    {
        services.AddDbContext<WikiQuizDbContext>(options =>
            options.UseNpgsql(Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection"),
                npgsqlOptions => npgsqlOptions.UseNodaTime()));

        // Register repositories 
        services.AddScoped<IQuizRepository, QuizRepository>();
        services.AddScoped<IWikipediaPageRepository, WikipediaPageRepository>();

        return services;
    }
}
