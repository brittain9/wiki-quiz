using WikiQuizGenerator.Core.DomainObjects;
using WikiQuizGenerator.Core.DTOs;

namespace WikiQuizGenerator.Core.Services;

public interface IPointsService
{
    int CalculatePointsEarned(IEnumerable<Question> questions, IEnumerable<QuestionAnswerDto> answers);
    int CalculateLevel(int totalPoints);
    int GetPointsRequiredForLevel(int level);
    int GetPointsRequiredForNextLevel(int currentLevel);
    (int scorePercent, int pointsEarned) CalculateScoreAndPoints(IEnumerable<Question> questions, IEnumerable<QuestionAnswerDto> answers);
}

public class PointsService : IPointsService
{
    private const int BasePointsPerLevel = 5000; // Points needed for level 2
    private const double LevelMultiplier = 1.5; // Each level requires 50% more points than the previous

        public int CalculatePointsEarned(IEnumerable<Question> questions, IEnumerable<QuestionAnswerDto> answers)
    {
        if (questions == null || answers == null) return 0;

            int totalPoints = 0;
        int index = 0;
        foreach (var question in questions)
        {
            var answer = answers.FirstOrDefault(a => a.QuestionId == index + 1);
            if (answer != null && answer.SelectedOptionNumber == (question.CorrectAnswerIndex + 1))
            {
                    totalPoints += 1000; // 1000 points per correct answer
            }
            index++;
        }

        return totalPoints;
    }

        public (int scorePercent, int pointsEarned) CalculateScoreAndPoints(IEnumerable<Question> questions, IEnumerable<QuestionAnswerDto> answers)
    {
        if (questions == null || answers == null) return (0, 0);
        var questionList = questions.ToList();
        if (questionList.Count == 0) return (0, 0);

            int correct = 0;
        for (int i = 0; i < questionList.Count; i++)
        {
            var q = questionList[i];
            var a = answers.FirstOrDefault(x => x.QuestionId == i + 1);
            if (a != null && (a.SelectedOptionNumber - 1) == q.CorrectAnswerIndex)
            {
                correct++;
            }
        }
        int scorePercent = (int)Math.Round((double)correct / questionList.Count * 100);
            int pointsEarned = correct * 1000; // 1000 points per correct answer
        return (scorePercent, pointsEarned);
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
