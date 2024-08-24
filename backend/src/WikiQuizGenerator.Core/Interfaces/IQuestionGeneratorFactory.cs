namespace WikiQuizGenerator.Core.Interfaces;

public interface IQuestionGeneratorFactory
{
    IQuestionGenerator Create(AiServiceManager aiServiceManager, int aiService, int model);
}