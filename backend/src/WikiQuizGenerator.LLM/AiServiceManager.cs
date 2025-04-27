using Microsoft.SemanticKernel.Services;
using System.Diagnostics.Contracts;
using System.Reflection;
using WikiQuizGenerator.Core.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WikiQuizGenerator.LLM;

public class AiServiceManager : IAiServiceManager
{
    public static bool IsOpenAiAvailable { get; private set; }
    public static string? OpenAiApiKey { get; set; }

    private readonly Dictionary<string, AiServiceConfig> _aiServices;
    public string? SelectedService { get; private set; }
    public string? SelectedModelId { get; private set; }

    public AiServiceManager()
    {
        OpenAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        IsOpenAiAvailable = !string.IsNullOrEmpty(OpenAiApiKey) && !OpenAiApiKey.Equals("YOUR_OPENAI_KEY_HERE");
        _aiServices = LoadAiServices();
    }

    private Dictionary<string, AiServiceConfig> LoadAiServices()
    {
        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "aiservices.json");
        if (!File.Exists(configPath))
            throw new FileNotFoundException($"AI services config not found at {configPath}");
        var json = File.ReadAllText(configPath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<Dictionary<string, AiServiceConfig>>(json, options) ?? new();
    }

    public void SelectAiService(int aiService, int model)
    {
        // aiService is the index in the available services list
        var availableServices = GetAvailableAiServices().ToList();
        if (aiService < 0 || aiService >= availableServices.Count)
            throw new ArgumentException("Invalid AI service selected.");
        SelectedService = availableServices[aiService].Value;
        var models = GetModels(aiService) as Dictionary<int, string>;
        if (models == null || !models.ContainsKey(model))
            throw new ArgumentException("Invalid model selected.");
        SelectedModelId = models[model];
    }

    public Dictionary<int, string> GetAvailableAiServices()
    {
        var availableServices = new Dictionary<int, string>();
        int idx = 0;
        if (IsOpenAiAvailable && _aiServices.ContainsKey("OpenAi"))
            availableServices.Add(idx++, "OpenAI");
        // Add more services here if needed in the future
        return availableServices;
    }

    public object GetModels(int? aiServiceId)
    {
        var availableServices = GetAvailableAiServices().ToList();
        if (!aiServiceId.HasValue || aiServiceId.Value < 0 || aiServiceId.Value >= availableServices.Count)
            return new { Error = "AI Service ID is required or invalid" };
        var serviceKey = availableServices[aiServiceId.Value].Value;
        if (!_aiServices.TryGetValue("OpenAi", out var openAiConfig))
            return new { Error = "Not Found" };
        var models = openAiConfig.Models;
        return models.ToDictionary(
            kvp => int.Parse(kvp.Key),
            kvp => kvp.Value.Name
        );
    }

    // Helper classes for deserialization
    public class AiServiceConfig
    {
        public Dictionary<string, ModelConfig> Models { get; set; } = new();
    }
    public class ModelConfig
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int MaxOutputTokens { get; set; }
        public int ContextWindow { get; set; } // dont really need this
        public double CostPer1MInputTokens { get; set; }
        public double CostPer1MCachedInputTokens { get; set; } // idk what this is
        public double CostPer1KOutputTokens { get; set; }
    }
}