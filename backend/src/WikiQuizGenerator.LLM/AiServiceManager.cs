using Microsoft.SemanticKernel.Services;
using System.Diagnostics.Contracts;
using System.Reflection;
using WikiQuizGenerator.Core.Interfaces;

namespace WikiQuizGenerator.LLM;

public class AiServiceManager : IAiServiceManager
{
    public static bool IsOpenAiAvailable { get; private set; }
    public static bool IsPerplexityAvailable { get; private set; }
    public static string? OpenAiApiKey { get; set; }
    public static string? PerplexityApiKey { get; set; }
    
    public AiService SelectedService { get; set; }
    public OpenAiModel SelectedOpenAiModel { get; set; }
    public PerplexityModel SelectedPerplexityModel { get; set; }

    public AiServiceManager()
    {
        OpenAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        PerplexityApiKey = Environment.GetEnvironmentVariable("PERPLEXITY_API_KEY");
    
        IsOpenAiAvailable = !string.IsNullOrEmpty(OpenAiApiKey) && !OpenAiApiKey.Equals("YOUR_OPENAI_KEY_HERE") ? true : false;
        IsPerplexityAvailable = !string.IsNullOrEmpty(PerplexityApiKey) && !PerplexityApiKey.Equals("YOUR_PERPLEXITY_KEY_HERE") ? true : false;
    }

    public void SelectAiService(int aiService, int model)
    {
        SelectedService = (AiService)aiService;

        switch (SelectedService)
        {
            case AiService.OpenAi:
                if (IsOpenAiAvailable)
                    SelectedOpenAiModel = (OpenAiModel)model;
                break;
            case AiService.Perplexity:
                if (IsPerplexityAvailable)
                    SelectedPerplexityModel = (PerplexityModel)model;
                break;
            default:
                throw new ArgumentException("Invalid AI service selected.");
        }
    }

    public Dictionary<int, string> GetAvailableAiServices()
    {
        var availableServices = new Dictionary<int, string>();

        if (AiServiceManager.IsOpenAiAvailable)
            availableServices.Add((int)AiService.OpenAi, "OpenAI");

        if (AiServiceManager.IsPerplexityAvailable)
            availableServices.Add((int)AiService.Perplexity, "Perplexity");

        return availableServices;
    }

    // Method to get models based on AI service ID
    public object GetModels(int? aiServiceId)
    {
        if (!aiServiceId.HasValue)
        {
            return new { Error = "AI Service ID is required" };
        }

        switch ((AiService)aiServiceId.Value)
        {
            case AiService.OpenAi:
                return OpenAiModelNames
                    .ToDictionary(kvp => (int)kvp.Key, kvp => kvp.Value);
            case AiService.Perplexity:
                return PerplexityModelNames
                    .ToDictionary(kvp => (int)kvp.Key, kvp => kvp.Value);
            default:
                return new { Error = "Not Found" };
        }
    }


    public static readonly Dictionary<PerplexityModel, string> PerplexityModelNames = new()
    {
        { PerplexityModel.Llama3_1_Sonar_Small_128k_Chat, "llama-3.1-sonar-small-128k-chat" },
        { PerplexityModel.Llama3_1_Sonar_Small_128k_Online, "llama-3.1-sonar-small-128k-online" },
        { PerplexityModel.Llama3_1_Sonar_Large_128k_Chat, "llama-3.1-sonar-large-128k-chat" },
        { PerplexityModel.Llama3_1_Sonar_Large_128k_Online, "llama-3.1-sonar-large-128k-online" },
        { PerplexityModel.Llama3_1_8b_Instruct, "llama-3.1-8b-instruct" },
        { PerplexityModel.Llama3_1_70b_Instruct, "llama-3.1-70b-instruct" }
    };

    public static readonly Dictionary<OpenAiModel, string> OpenAiModelNames = new()
    {
        { OpenAiModel.Gpt4oMini, "gpt-4o-mini" },
        { OpenAiModel.Gpt35Turbo, "gpt-3.5-turbo" }
    };
}