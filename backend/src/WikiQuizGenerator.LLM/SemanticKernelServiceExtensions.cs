using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.TextGeneration;
using WikiQuizGenerator.Core;

namespace WikiQuizGenerator.LLM;

// I currently only inject the kernel into the QuestionGenerator class of this same project. Do we need service extensions here?
// It seems I could just create the kernel in the constructor which would be more efficient.
// I'll rework this later.
public static class SemanticKernelServiceExtensions
{
    // the user will be shown a dropdown of available services in the frontend
    public static AiService SelectedService { get; set; }
    public static OpenAiModel SelectedOpenAiModel { get; set; } = OpenAiModel.Gpt4oMini;
    public static PerplexityModel SelectedPerplexityModel { get; set; } = PerplexityModel.Llama3_1_8b_Instruct;
    public static bool IsOpenAiAvailable { get; private set; }
    public static bool IsPerplexityAvailable { get; private set; }
    
    public static IServiceCollection AddAiService(this IServiceCollection services, IConfiguration configuration)
    {
        string? openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        string? perplexityApiKey = Environment.GetEnvironmentVariable("PERPLEXITY_API_KEY");
    
        IsOpenAiAvailable = !string.IsNullOrEmpty(openAiApiKey) && !openAiApiKey.Equals("YOUR_OPENAI_KEY_HERE") ? true : false;
        IsPerplexityAvailable = !string.IsNullOrEmpty(perplexityApiKey) && !perplexityApiKey.Equals("YOUR_PERPLEXITY_KEY_HERE") ? true : false;

        string modelId = SelectedService switch
        {
            AiService.OpenAi => AiServiceManager.OpenAiModelNames[SelectedOpenAiModel],
            AiService.Perplexity => AiServiceManager.PerplexityModelNames[SelectedPerplexityModel],
            _ => throw new ArgumentException("Invalid AI service selected.")
        };
        // Each request will create a new kernel with the specified chat completion service.
        services.AddTransient(sp =>
        {
            var kernelBuilder = Kernel.CreateBuilder();
        
            switch (SelectedService)
            {
                case AiService.OpenAi:
                    if (IsOpenAiAvailable)
                        kernelBuilder.AddOpenAIChatCompletion(modelId, openAiApiKey);
                    break;
                case AiService.Perplexity:
                    if (IsPerplexityAvailable)
                        kernelBuilder.AddPerplexityAIChatCompletion(modelId, perplexityApiKey);
                    break;
                default:
                    throw new ArgumentException("Invalid AI service selected.");
            }
        
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