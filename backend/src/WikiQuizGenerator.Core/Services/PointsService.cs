using WikiQuizGenerator.Core.Models;
using WikiQuizGenerator.Core.DTOs;

namespace WikiQuizGenerator.Core.Services;

public interface IPointsService
{
    int CalculatePointsEarned(IEnumerable<Question> questions, IEnumerable<QuestionAnswerDto> answers);
    int CalculateLevel(int totalPoints);
    int GetPointsRequiredForLevel(int level);
    int GetPointsRequiredForNextLevel(int currentLevel);
}

public class PointsService : IPointsService
{
    private const int BasePointsPerLevel = 5000; // Points needed for level 2
    private const double LevelMultiplier = 1.5; // Each level requires 50% more points than the previous

    public int CalculatePointsEarned(IEnumerable<Question> questions, IEnumerable<QuestionAnswerDto> answers)
    {
        if (questions == null || answers == null) return 0;

        int totalPoints = 0;
        
        foreach (var question in questions)
        {
            var answer = answers.FirstOrDefault(a => a.QuestionId == question.Id);
            if (answer != null && answer.SelectedOptionNumber == question.CorrectOptionNumber)
            {
                totalPoints += question.PointValue;
            }
        }

        return totalPoints;
    }

    public int CalculateLevel(int totalPoints)
    {
        if (totalPoints < BasePointsPerLevel) return 1;

        int level = 1;
        int pointsNeeded = 0;

        while (pointsNeeded <= totalPoints)
        {
            level++;
            pointsNeeded += GetPointsRequiredForLevel(level);
        }

        return level - 1; // Return the last level we actually achieved
    }

    public int GetPointsRequiredForLevel(int level)
    {
        if (level <= 1) return 0;
        
        // Level 2 requires BasePointsPerLevel points
        // Each subsequent level requires more points based on the multiplier
        return (int)(BasePointsPerLevel * Math.Pow(LevelMultiplier, level - 2));
    }

    public int GetPointsRequiredForNextLevel(int currentLevel)
    {
        return GetPointsRequiredForLevel(currentLevel + 1);
    }
}
