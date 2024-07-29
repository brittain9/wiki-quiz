using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Core.Interfaces;

public interface IQuestionGenerator
{
    Task<QuestionResponse> GenerateQuestionsAsync(WikipediaPage text, string language, int numberOfQuestions, int textSubstringLength);
}