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

    public IQuestionGenerator Create(AiServiceManager aiServiceManager, int aiService, int model)
    {
        var promptManager = _serviceProvider.GetRequiredService<PromptManager>();
        var logger = _serviceProvider.GetRequiredService<ILogger<QuestionGenerator>>();
        
        aiServiceManager.SelectedService = (AiService)aiService;
        
        switch (aiServiceManager.SelectedService)
        {
            case AiService.OpenAi:
                if (AiServiceManager.IsOpenAiAvailable)
                    aiServiceManager.SelectedOpenAiModel = (OpenAiModel)model;
                break;
            case AiService.Perplexity:
                if (AiServiceManager.IsPerplexityAvailable)
                    aiServiceManager.SelectedPerplexityModel = (PerplexityModel)model;
                break;
            default:
                throw new ArgumentException("Invalid AI service selected.");
        }
        
        return new QuestionGenerator(promptManager, logger, aiServiceManager);
    }
}