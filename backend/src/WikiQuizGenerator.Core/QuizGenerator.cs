using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
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
        WikipediaPage page = await _wikipediaContentProvider.GetWikipediaPage(topic, language); // throws exception if topic not found

        if (page == null) // The topic was not found on Wikipedia
            return null;

        Quiz quiz = new Quiz();

        quiz.Title = page.Title;

        var content = RandomContentSections.GetRandomContentSections(page.Extract, extractLength);

        var aiResponse = await _questionGenerator.GenerateQuestionsAsync(page, content, language, numQuestions, numOptions);
        quiz.AIResponses.Add(aiResponse);
        quiz.CreatedAt = DateTime.UtcNow;

        var result = await _quizRepository.AddAsync(quiz);
        return result;
    }
}
