using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WikiQuizGenerator.Core.Models;

public class ModelConfig
{
    public int Id { get; set; } // database id
    public string ModelId { get; set; } // id for open ai api
    public string Name { get; set; }
    public int MaxTokens { get; set; }
    public int ContextWindow { get; set; }
    public double CostPer1MInputTokens { get; set; }
    public double CostPer1MCachedInputTokens { get; set; }
    public double CostPer1MOutputTokens { get; set; }
}
