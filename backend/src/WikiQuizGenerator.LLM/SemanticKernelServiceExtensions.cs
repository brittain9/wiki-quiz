using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.TextGeneration;

namespace WikiQuizGenerator.LLM;

// I currently only inject the kernel into the QuestionGenerator class of this same project. Do we need service extensions here?
// It seems I could just create the kernel in the constructor which would be more efficient.
// I'll rework this later.
public static class SemanticKernelServiceExtensions
{ 
    public static bool IsOpenAiAvailable { get; private set; }
    public static bool IsPerplexityAvailable { get; private set; }
    public static IServiceCollection AddAiService(this IServiceCollection services, IConfiguration configuration, string modelId="gpt-4o-mini")
    {
        string? openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        string? perplexityApiKey = Environment.GetEnvironmentVariable("PERPLEXITY_API_KEY");
        
        IsOpenAiAvailable = !string.IsNullOrEmpty(openAiApiKey) && !openAiApiKey.Equals("YOUR_OPENAI_KEY_HERE") ? true : false;
        IsPerplexityAvailable = !string.IsNullOrEmpty(perplexityApiKey) && !perplexityApiKey.Equals("YOUR_PERPLEXITY_KEY_HERE") ? true : false;
        
        services.AddSingleton(sp =>
        {
            var kernelBuilder = Kernel.CreateBuilder();
            
            if (IsOpenAiAvailable)
                kernelBuilder.AddOpenAIChatCompletion(modelId, openAiApiKey!, serviceId: "openai");
            if (IsPerplexityAvailable)
                kernelBuilder.AddPerplexityAIChatCompletion(modelId, perplexityApiKey!, serviceId: "perplexity");
            
            return kernelBuilder.Build();
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