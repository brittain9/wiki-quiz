using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WikiQuizGenerator.Core.Models;

public class AIResponse
{
    public int Id { get; set; }

    public IList<Question> Questions { get; set; }

    public long ResponseTime { get; set; } // in milliseconds
    public int? InputTokenCount { get; set; }
    public int? OutputTokenCount { get; set; }

    public int ModelConfigId { get; set; }
    public ModelConfig ModelConfig { get; set; }

    // Navigational Property for one-to-one relationship between ai response and wikipedia page
    public int WikipediaPageId { get; set; }
    public WikipediaPage WikipediaPage { get; set; }

    // Navigational Property for one to many relationship between quiz and ai response.
    public int QuizId { get; set; }
    
    [JsonIgnore]
    public Quiz Quiz { get; set; }

    public double CalculateCost()
    {
        if (InputTokenCount == null || OutputTokenCount == null || ModelConfig == null ||
            ModelConfig.CostPer1MInputTokens == null || ModelConfig.CostPer1KOutputTokens == null)
        {
            return 0.0;
        }

        double inputCost = (InputTokenCount.Value / 1_000_000.0) * ModelConfig.CostPer1MInputTokens;
        double outputCost = (OutputTokenCount.Value / 1_000_000.0) * ModelConfig.CostPer1KOutputTokens;

        return inputCost + outputCost;
    }
}
