using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WikiQuizGenerator.Core;
using WikiQuizGenerator.Core.Interfaces;

namespace WikiQuizGenerator.LLM;

public class QuestionGeneratorFactory : IQuestionGeneratorFactory
{
    private readonly IServiceProvider _serviceProvider;

    public QuestionGeneratorFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IQuestionGenerator Create(IAiServiceManager aiServiceManager, string aiService, string model)
    {
        var promptManager = _serviceProvider.GetRequiredService<PromptManager>();
        var logger = _serviceProvider.GetRequiredService<ILogger<QuestionGenerator>>();

        aiServiceManager.SelectAiService(aiService, model);
        
        // TODO: I dont love the cast here
        return new QuestionGenerator(promptManager, (AiServiceManager)aiServiceManager, logger);
    }
}