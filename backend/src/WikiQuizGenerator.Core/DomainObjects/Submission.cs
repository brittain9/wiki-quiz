using WikiQuizGenerator.Core.DomainObjects;

namespace WikiQuizGenerator.Core.DomainObjects;

/// <summary>
/// Domain object representing a quiz submission with user answers
/// </summary>
public class Submission
{
    public Guid UserId { get; set; }
    public DateTime SubmissionTime { get; set; }
    public List<int> Answers { get; set; } = new(); // 0-based indices corresponding to question order and selected options
    public int Score { get; set; }
    public int PointsEarned { get; set; }
}