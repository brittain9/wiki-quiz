using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace WikiQuizGenerator.Data;

public static class DataServiceExtensions
{
    public static IServiceCollection AddDataServices(this IServiceCollection services)
    {
        services.AddDbContext<WikiQuizDbContext>(options =>
            options.UseNpgsql(Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")));

        // Register repositories
        // services.AddScoped<IQuizRepository, QuizRepository>();
        // services.AddScoped<IQuestionRepository, QuestionRepository>();

        // Add other services
        // services.AddScoped<ISomeService, SomeService>();

        return services;
    }
}
