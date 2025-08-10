using WikiQuizGenerator.Core.DomainObjects;
using WikiQuizGenerator.Core.Utilities;

namespace WikiQuizGenerator.Core.Interfaces;

public interface IQuizGenerator
{
    Task<Quiz> GenerateQuizAsync(
        string topic, 
        Languages language, 
        string aiService,
        string model, 
        int numQuestions, 
        int numOptions, 
        int extractLength,
        Guid createdBy,
        CancellationToken cancellation = default);
}

