using System.Text.Json;
using WikiQuizGenerator.Core.DomainObjects;
using WikiQuizGenerator.Core.Interfaces;

namespace WikiQuizGenerator.LLM;

public class AiServiceManager : IAiServiceManager
{
    public static bool IsOpenAiAvailable { get; private set; }
    public static string? OpenAiApiKey { get; set; }

    public readonly Dictionary<string, List<ModelConfig>> AiServices;
    public string? SelectedService { get; private set; }
    public string? SelectedModelId { get; private set; }

    public AiServiceManager(string openAIApiKey)
    {
        OpenAiApiKey = openAIApiKey;
        IsOpenAiAvailable = !string.IsNullOrEmpty(OpenAiApiKey);
        // Lazily load AI services to avoid I/O during cold start;
        AiServices = new Dictionary<string, List<ModelConfig>>(StringComparer.OrdinalIgnoreCase);
    }

    private Dictionary<string, List<ModelConfig>> LoadAiServices()
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

            var dict = new Dictionary<string, List<ModelConfig>>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in rawData)
            {
                dict[kvp.Key] = kvp.Value;
            }
            return dict;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Failed to parse AI services configuration.", ex);
        }
    }

    public void SelectAiService(string aiServiceId, string modelName)
    {
        if (AiServices.Count == 0)
        {
            var loaded = LoadAiServices();
            foreach (var kv in loaded)
                AiServices[kv.Key] = kv.Value;
        }
        if (!AiServices.TryGetValue(aiServiceId, out var serviceConfig))
            throw new ArgumentException("Invalid AI service selected.");

        var model = serviceConfig.FirstOrDefault(m => m.Name == modelName);
        if (model == null)
            throw new ArgumentException("Invalid model selected.");

        SelectedService = aiServiceId;
        SelectedModelId = model.ModelId;
    }

    public string[] GetAvailableAiServices()
    {
        if (AiServices.Count == 0)
        {
            var loaded = LoadAiServices();
            foreach (var kv in loaded)
                AiServices[kv.Key] = kv.Value;
        }
        return AiServices.Keys.ToArray();

    }

    public string[] GetModels(string aiServiceId)
    {
        if (AiServices.Count == 0)
        {
            var loaded = LoadAiServices();
            foreach (var kv in loaded)
                AiServices[kv.Key] = kv.Value;
        }
        if (!AiServices.TryGetValue(aiServiceId, out var serviceConfig))
            return Array.Empty<string>();

        return serviceConfig.Select(model => model.Name).ToArray();
    }
}