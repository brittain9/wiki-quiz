using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.TextGeneration;
using WikiQuizGenerator.Core;

namespace WikiQuizGenerator.LLM;

public static class SemanticKernelServiceExtensions
{
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