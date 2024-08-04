using Microsoft.Extensions.Logging;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Core;

public class QuizGenerator : IQuizGenerator
{
    private IWikipediaContentProvider _wikipediaContentProvider;
    private readonly IQuizRepository _quizRepository;
    private readonly IQuestionGenerator _questionGenerator;
    private readonly ILogger<QuizGenerator> _logger;

    public QuizGenerator(IQuestionGenerator questionGenerator, IWikipediaContentProvider wikipediaContentProvider, ILogger<QuizGenerator> logger, IQuizRepository quizRepository)
    {
        _wikipediaContentProvider = wikipediaContentProvider;
        _questionGenerator = questionGenerator;
        _logger = logger;
        _quizRepository = quizRepository;
    }

    public async Task<Quiz> GenerateBasicQuizAsync(string topic, Languages language, int numQuestions, int numOptions, int extractLength)
    {
        _logger.LogTrace($"Generating a basic quiz on '{topic}' in {language.GetWikipediaLanguageCode()} with {numQuestions} questions, {numOptions} options, and {extractLength} extract length.");

        WikipediaPage page = await _wikipediaContentProvider.GetWikipediaPage(topic, language);
        if (page == null)
            return null; // we got pages, but they aren't valid. Should never happen really, so may change later

        var content = RandomContentSections.GetRandomContentSections(page.Extract, extractLength);

        var aiResponse = await _questionGenerator.GenerateQuestionsAsync(page, content, language, numQuestions, numOptions);
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
