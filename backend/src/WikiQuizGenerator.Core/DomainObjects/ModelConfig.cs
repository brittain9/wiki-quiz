using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WikiQuizGenerator.Core.DomainObjects;

/// <summary>
/// Domain object representing a model configuration
/// </summary>
public class ModelConfig
{
    public string ModelId { get; set; } = string.Empty; // id for open ai api
    public string Name { get; set; } = string.Empty;
    public int MaxTokens { get; set; }
    public int ContextWindow { get; set; }
    public double CostPer1MInputTokens { get; set; }
    public double CostPer1MCachedInputTokens { get; set; }
    public double CostPer1MOutputTokens { get; set; }
}
