namespace WikiQuizGenerator.Data.Cosmos.Entities;

public sealed class SubmissionDocument
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime SubmissionTime { get; set; }
    public int Score { get; set; }
    public int PointsEarned { get; set; }
    public List<int> Answers { get; set; } = new();
}


