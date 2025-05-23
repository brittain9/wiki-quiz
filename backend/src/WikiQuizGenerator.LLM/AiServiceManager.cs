using System.Text.Json;
using WikiQuizGenerator.Core.Models;
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
        AiServices = LoadAiServices();
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

            return rawData.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value
            );
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Failed to parse AI services configuration.", ex);
        }
    }

    public void SelectAiService(string aiServiceId, string modelName)
    {
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
        return AiServices.Keys.ToArray();

    }

    public string[] GetModels(string aiServiceId)
    {
        if (!AiServices.TryGetValue(aiServiceId, out var serviceConfig))
            return Array.Empty<string>();

        return serviceConfig.Select(model => model.Name).ToArray();
    }
}