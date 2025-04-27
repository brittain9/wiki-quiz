using System.Text.Json;
using WikiQuizGenerator.Core.Interfaces;

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
        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "aiservices.json");
        if (!File.Exists(configPath))
            throw new FileNotFoundException($"AI services config not found at {configPath}");

        try
        {
            var json = File.ReadAllText(configPath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var rawData = JsonSerializer.Deserialize<Dictionary<string, List<ModelConfig>>>(json, options);
            if (rawData == null)
                throw new InvalidOperationException("The AI services configuration file is empty or invalid.");

            return rawData.ToDictionary(
                kvp => kvp.Key,
                kvp => new AiServiceConfig { Models = kvp.Value }
            );
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Failed to parse AI services configuration.", ex);
        }
    }

    public void SelectAiService(string aiServiceId, string modelName)
    {
        if (!_aiServices.TryGetValue(aiServiceId, out var serviceConfig))
            throw new ArgumentException("Invalid AI service selected.");

        var model = serviceConfig.Models.FirstOrDefault(m => m.Name == modelName);
        if (model == null)
            throw new ArgumentException("Invalid model selected.");

        SelectedService = aiServiceId;
        SelectedModelId = model.Id;
    }

    public string[] GetAvailableAiServices()
    {
        return _aiServices.Keys.ToArray();

    }

    public string[] GetModels(string aiServiceId)
    {
        if (!_aiServices.TryGetValue(aiServiceId, out var serviceConfig))
            return Array.Empty<string>();

        return serviceConfig.Models.Select(model => model.Name).ToArray();
    }

    // Helper classes for deserialization
    public class AiServiceConfig
    {
        public List<ModelConfig> Models { get; set; } = new();
    }

    public class ModelConfig
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int MaxOutputTokens { get; set; }
        public int ContextWindow { get; set; }
        public double CostPer1MInputTokens { get; set; }
        public double CostPer1MCachedInputTokens { get; set; }
        public double CostPer1KOutputTokens { get; set; }
    }
}