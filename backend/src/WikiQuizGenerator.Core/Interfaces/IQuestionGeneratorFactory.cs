namespace WikiQuizGenerator.Core.Interfaces;

public interface IQuestionGeneratorFactory
{
    IQuestionGenerator Create(IAiServiceManager aiServiceManager, int aiService, int model);
}