using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.LLM;
using WikiQuizGenerator.Data;
using WikiQuizGenerator.Core;
using Serilog;

public partial class Program
{
    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        
        services.AddSerilog((services, lc) => lc
            .ReadFrom.Configuration(configuration) // appsettings.json
            .ReadFrom.Services(services));

        services.AddDataServices();
        services.AddScoped<IWikipediaContentProvider, WikipediaContentProvider>();
        services.AddSingleton<PromptManager>();
        services.AddScoped<AiServiceManager>();
        
        services.AddSingleton<IQuestionGeneratorFactory, QuestionGeneratorFactory>();
        services.AddTransient<IQuizGenerator, QuizGenerator>();
        
        services.AddCors(options =>
        {
            options.AddPolicy("AllowReactApp",
                builder => builder
                    .WithOrigins("http://localhost:5173") // React app's URL
                    .AllowAnyMethod()
                    .AllowAnyHeader());
        });
    }
}