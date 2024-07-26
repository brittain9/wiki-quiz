using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace WikiQuizGenerator.LLM;

public static class SemanticKernelServiceExtensions
{
    public static IServiceCollection AddOpenAIService(this IServiceCollection services, IConfiguration configuration, string modelId="gpt-4o-mini")
    {
        string? openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

        if(string.IsNullOrEmpty(openAiApiKey))
        {
            throw new InvalidOperationException("OpenAI API key not found in environment variables. Please configure the .env file");
        }

        services.AddSingleton(sp =>
        {
            var kernelBulder = Kernel.CreateBuilder()
                .AddOpenAIChatCompletion(modelId, openAiApiKey);

            return kernelBulder.Build();
        });

        return services;
    }

    public static IServiceCollection AddPerplexityAIService(this IServiceCollection services, IConfiguration configuration, string modelId = "")
    {
        string? perplexityApiKey = Environment.GetEnvironmentVariable("PERPLEXITY_API_KEY");

        if (string.IsNullOrEmpty(perplexityApiKey))
        {
            throw new InvalidOperationException("Perplexity API key not found in environment variables. Please configure the .env file");
        }

        // Create the perplexity chat completion class

        return services;
    }
}