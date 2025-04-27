namespace WikiQuizGenerator.Core.Interfaces;

public interface IQuestionGeneratorFactory
{
    IQuestionGenerator Create(IAiServiceManager aiServiceManager, string aiService, string model);
}