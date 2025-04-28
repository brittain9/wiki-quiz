using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Core.Interfaces;

public interface IQuestionGenerator
{
    Task<AIResponse> GenerateQuestionsAsync(WikipediaPage text, string content, Languages language, int numberOfQuestions, int numOptions, CancellationToken cancellationToken);
}