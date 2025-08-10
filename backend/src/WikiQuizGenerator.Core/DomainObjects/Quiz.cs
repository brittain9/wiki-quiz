namespace WikiQuizGenerator.Core.DomainObjects;

/// <summary>
/// Domain object representing a quiz - simplified for Cosmos DB
/// </summary>
public class Quiz
{
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public List<Question> Questions { get; set; } = new();
    public WikipediaReference? WikipediaReference { get; set; }
    public List<Submission> Submissions { get; set; } = new();
}}