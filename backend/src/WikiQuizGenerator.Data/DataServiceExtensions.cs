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
            options.UseNpgsql(Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")));

        // Register repositories
        services.AddScoped<IQuizRepository, QuizRepository>();
        services.AddScoped<IQuestionResponseRepository, QuestionResponseRepository>();
        services.AddScoped<IQuestionRepository, QuestionRepository>();
        services.AddScoped<IWikipediaPageRepository, WikipediaPageRepository>();

        return services;
    }
}
