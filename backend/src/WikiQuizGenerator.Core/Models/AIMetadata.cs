using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WikiQuizGenerator.Core.Models;

public class AIMetadata
{
    [Key]
    public int Id { get; set; }

    public long ResponseTime { get; set; } // in milliseconds
    public int? PromptTokenUsage { get; set; }
    public int? CompletionTokenUsage { get; set; }
    public string? ModelName { get; set; }

    public int AIResponseId { get; set; }
    public AIResponse AIResponse { get; set; }
}