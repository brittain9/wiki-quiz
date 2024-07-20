using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Core.Interfaces;

public interface IQuestionGenerator
{
    Task<List<Question>> GenerateQuestionsAsync(string text, int numberOfQuestions, int textSubstringLength);
}