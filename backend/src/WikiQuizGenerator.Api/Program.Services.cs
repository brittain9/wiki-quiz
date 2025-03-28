using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.LLM;
using WikiQuizGenerator.Data;
using WikiQuizGenerator.Core;

public partial class Program
{
    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddDataServices();

        // TODO: These lifetimes could use work
        services.AddScoped<IWikipediaContentProvider, WikipediaContentProvider>();
        services.AddSingleton<PromptManager>();
        services.AddScoped<IAiServiceManager, AiServiceManager>();
        
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