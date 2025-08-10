using WikiQuizGenerator.Core.DomainObjects;
using WikiQuizGenerator.Core.Services;
using WikiQuizGenerator.Core.Utilities;

namespace WikiQuizGenerator.Core.Interfaces;

public interface IQuestionGenerator
{
    Task<QuestionGenerationResult> GenerateQuestionsAsync(
        string content, 
        Languages language, 
        int numberOfQuestions, 
        int numOptions, 
        CancellationToken cancellationToken);
}