using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Core.Interfaces;

public interface IQuestionGenerator
{
    Task<AIResponse> GenerateQuestionsAsync(WikipediaPage text, string language, int numberOfQuestions, int numOptions, int textSubstringLength);
}