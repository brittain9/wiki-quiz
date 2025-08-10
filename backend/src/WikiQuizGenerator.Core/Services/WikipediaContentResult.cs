using WikiQuizGenerator.Core.DomainObjects;

namespace WikiQuizGenerator.Core.Services;

/// <summary>
/// Result returned by Wikipedia content service containing reference and processed content
/// </summary>
public class WikipediaContentResult
{
    public WikipediaReference WikipediaReference { get; set; } = new();
    public string ProcessedContent { get; set; } = string.Empty;
}