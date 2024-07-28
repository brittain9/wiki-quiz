using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Core.Interfaces;

public interface IQuestionGenerator
{
    Task<QuestionResponse> GenerateQuestionsAsync(WikipediaPage text, int numberOfQuestions, int textSubstringLength = 500, string language = "en");
}