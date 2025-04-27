using Microsoft.Extensions.Logging;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Core;

public class QuizGenerator : IQuizGenerator
{
    private IWikipediaContentProvider _wikipediaContentProvider;
    private readonly IQuizRepository _quizRepository;
    private readonly IAiServiceManager _aiServiceManager;
    private readonly IQuestionGeneratorFactory _questionGeneratorFactory;
    private readonly ILogger<QuizGenerator> _logger;

    public QuizGenerator(IQuestionGeneratorFactory questionGeneratorFactory, IWikipediaContentProvider wikipediaContentProvider, IAiServiceManager aiServiceManager, ILogger<QuizGenerator> logger, IQuizRepository quizRepository)
    {
        _wikipediaContentProvider = wikipediaContentProvider;
        _questionGeneratorFactory = questionGeneratorFactory;
        _aiServiceManager = aiServiceManager;
        _logger = logger;
        _quizRepository = quizRepository;
    }

    public async Task<Quiz> GenerateBasicQuizAsync(string topic, Languages language, string aiService, string model, int numQuestions, int numOptions, int extractLength)
    {
        _logger.LogTrace($"Generating a basic quiz on '{topic}' in '{language.GetWikipediaLanguageCode()}' with {numQuestions} questions, {numOptions} options, and {extractLength} extract length.");
        
        WikipediaPage page = await _wikipediaContentProvider.GetWikipediaPage(topic, language); // throws error and returns result in middleware if page doesn't exist

        var content = RandomContentSections.GetRandomContentSections(page.Extract, extractLength);

        var questionGenerator = _questionGeneratorFactory.Create(_aiServiceManager, aiService, model);

        var aiResponse = await questionGenerator.GenerateQuestionsAsync(page, content, language, numQuestions, numOptions);
        if (aiResponse == null)
            return null;

        Quiz quiz = new Quiz()
        {
            Title = page.Title,
            CreatedAt = DateTime.UtcNow,
            AIResponses = new List<AIResponse> { aiResponse }
        };

        var result = await _quizRepository.AddAsync(quiz);
        return result;
    }
}
