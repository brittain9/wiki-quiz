using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace WikiQuizGenerator.LLM;

public static class SemanticKernelServiceExtensions
{
    public static IServiceCollection AddLLMService(this IServiceCollection services, IConfiguration configuration)
    {
        string? openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if(string.IsNullOrEmpty(openAiApiKey))
        {
            throw new InvalidOperationException("OpenAI API key not found in environment variables. Please configure the .env file");
        }

        // Configure Semantic Kernel
        services.AddSingleton(sp =>
        {
            var kernelBuilder = Kernel.CreateBuilder()
                .AddOpenAIChatCompletion("gpt-3.5-turbo", openAiApiKey);
            
            return kernelBuilder.Build();
        });

        return services;
    }
}