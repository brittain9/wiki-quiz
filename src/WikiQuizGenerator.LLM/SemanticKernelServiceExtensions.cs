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
}