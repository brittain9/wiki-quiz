namespace WikiQuizGenerator.Core.DomainObjects;

/// <summary>
/// Simple Wikipedia reference containing only essential information needed for quiz display
/// </summary>
public class WikipediaReference
{
    public int PageId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
}