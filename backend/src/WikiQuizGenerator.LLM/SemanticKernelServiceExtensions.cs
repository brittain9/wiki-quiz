using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.TextGeneration;

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

    public static IServiceCollection AddPerplexityAIService(this IServiceCollection services, IConfiguration configuration, string modelId = "llama-3.1-sonar-small-128k-chat")
    {
        string? perplexityApiKey = Environment.GetEnvironmentVariable("PERPLEXITY_API_KEY");
        
        if (string.IsNullOrEmpty(perplexityApiKey))
        {
            throw new InvalidOperationException("Perplexity API key not found in environment variables. Please configure the .env file");
        }

        services.AddSingleton(sp =>
        {
            var kernelBulder = Kernel.CreateBuilder()
                .AddPerplexityAIChatCompletion(modelId, perplexityApiKey);

            return kernelBulder.Build();
        });

        return services;
    }

    public static IKernelBuilder AddPerplexityAIChatCompletion(
        this IKernelBuilder builder,
        string modelId,
        string apiKey,
        string? serviceId = null,
        HttpClient? httpClient = null)
    {
        // this method registers the PerplexityChatCompetion service with the KernelBuilder to use in the service extension
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        Func<IServiceProvider, object?, PerplexityAIChatCompletion> factory = (serviceProvider, _) =>
        {
            var client = httpClient ?? new HttpClient();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var logger = loggerFactory?.CreateLogger<PerplexityAIChatCompletion>();
            return new PerplexityAIChatCompletion(apiKey, modelId, httpClient: client, logger: logger);
        };

        builder.Services.AddKeyedSingleton<IChatCompletionService>(serviceId, factory);

        return builder;
    }
}