using System.Text.Json;
using Microsoft.Extensions.Logging;
using WikiQuizGenerator.Core.DomainObjects;

namespace WikiQuizGenerator.Core.Services;

/// <summary>
/// In-memory service for ModelConfig data loaded from JSON file.
/// This replaces the database-stored ModelConfig data for the Cosmos DB version.
/// </summary>
public interface IModelConfigService
{
    Task<ModelConfig?> GetByIdAsync(int id);
    Task<IEnumerable<ModelConfig>> GetAllAsync();
    Task<ModelConfig?> GetByModelIdAsync(string modelId);
}

public class ModelConfigService : IModelConfigService
{
    private readonly List<ModelConfig> _modelConfigs;
    private readonly ILogger<ModelConfigService> _logger;

    public ModelConfigService(ILogger<ModelConfigService> logger)
    {
        _logger = logger;
        _modelConfigs = new List<ModelConfig>();
        LoadModelConfigs();
    }

    private void LoadModelConfigs()
    {
        try
        {
            // Look for the JSON file in the expected locations
            var possiblePaths = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "aiservices.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "aiservices.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "src", "WikiQuizGenerator.LLM", "aiservices.json"),
                Path.Combine("..", "WikiQuizGenerator.LLM", "aiservices.json")
            };

            string? configPath = possiblePaths.FirstOrDefault(File.Exists);
            
            if (configPath == null)
            {
                _logger.LogWarning("aiservices.json not found in any expected location. ModelConfig service will return empty results.");
                return;
            }

            _logger.LogInformation("Loading ModelConfig data from: {ConfigPath}", configPath);

            var json = File.ReadAllText(configPath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var rawData = JsonSerializer.Deserialize<Dictionary<string, List<ModelConfig>>>(json, options);
            if (rawData != null)
            {
                var allConfigs = rawData.SelectMany(kvp => kvp.Value).ToList();
                _modelConfigs.AddRange(allConfigs);
                
                _logger.LogInformation("Loaded {Count} ModelConfig entries", _modelConfigs.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load ModelConfig data from JSON file");
        }
    }

    public Task<ModelConfig?> GetByIdAsync(int id)
    {
        var config = _modelConfigs.FirstOrDefault(m => m.Id == id);
        return Task.FromResult(config);
    }

    public Task<IEnumerable<ModelConfig>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<ModelConfig>>(_modelConfigs);
    }

    public Task<ModelConfig?> GetByModelIdAsync(string modelId)
    {
        var config = _modelConfigs.FirstOrDefault(m => m.ModelId == modelId);
        return Task.FromResult(config);
    }
}