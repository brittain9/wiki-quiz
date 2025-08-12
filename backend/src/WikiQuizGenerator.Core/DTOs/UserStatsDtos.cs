namespace WikiQuizGenerator.Core.DTOs;

public sealed class UserUsageDto
{
    public Guid UserId { get; set; }
    public bool IsPremium { get; set; }
    public double CurrentCost { get; set; }
    public double WeeklyCostLimit { get; set; }
    public double Remaining { get; set; }
    public int PeriodDays { get; set; }
}

public sealed class UserStatsDto
{
    public Guid UserId { get; set; }
    public int TotalPoints { get; set; }
    public int Level { get; set; }
    public int NextLevel { get; set; }
    public int PointsRequiredForNextLevel { get; set; }
    public int PointsToNextLevel { get; set; }
}


