using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    private readonly IQuestionGenerator _questionGenerator;
    private readonly ILogger<QuizGenerator> _logger;

    public QuizGenerator(IQuestionGenerator questionGenerator, IWikipediaContentProvider wikipediaContentProvider, ILogger<QuizGenerator> logger)
    {
        _wikipediaContentProvider = wikipediaContentProvider;
        _questionGenerator = questionGenerator;
        _logger = logger;
    }

    public async Task<Quiz> GenerateBasicQuizAsync(string topic, string language, int numQuestions, int numOptions, int extractLength)
    {
        Languages lang = LanguagesExtensions.GetLanguageFromCode(language); // this will throw error if language is not found

        WikipediaPage page = await _wikipediaContentProvider.GetWikipediaPage(topic, lang); // throws exception if topic not found

        if(page == null) // The topic was not found on Wikipedia
            return null; // This will fail fast, and ill indicate in text box that the topic is invalid within a second.

        Quiz quiz = new Quiz();

        quiz.Title = page.Title;

        var content = GetRandomContentSections(page, extractLength);

        var aiResponse = await _questionGenerator.GenerateQuestionsAsync(page, content, language, numQuestions, numOptions);
        quiz.AIResponses.Add(aiResponse);
        quiz.CreatedAt = DateTime.UtcNow;

        return quiz;
    }

    private static string GetRandomContentSections(WikipediaPage page, int requestedContentLength)
    {
        const int MIN_CONTENT_LENGTH = 500; // minimum content for quality questions
        const int MAX_CONTENT_LENGTH = 50000; // max to avoid using too many prompt tokens. 50k is super generous too
        const int LENGTH_FOR_LARGER_SECTIONS = 3000; // when doing large amounts of content, use bigger sections
        const int LARGER_SECTION_SIZE = 1000;
        const int SMALLER_SECTION_SIZE = 500;

        int contentLength = Math.Clamp(requestedContentLength, MIN_CONTENT_LENGTH, MAX_CONTENT_LENGTH);
        string extract = page.Extract;

        // If requested length is greater than or equal to the extract length, return the full extract
        if (contentLength >= extract.Length)
        {
            return extract;
        }

        int sectionSize = contentLength >= LENGTH_FOR_LARGER_SECTIONS ? LARGER_SECTION_SIZE : SMALLER_SECTION_SIZE;

        Random random = new Random();
        StringBuilder contentSb = new StringBuilder(contentLength);
        HashSet<int> usedPositions = new HashSet<int>();

        while (contentLength > 0)
        {
            int remainingLength = extract.Length - usedPositions.Count;
            if (remainingLength <= 0) break;

            int maxStartPosition = Math.Max(0, remainingLength - sectionSize);
            int startPosition;

            do
            {
                startPosition = random.Next(maxStartPosition + 1);
            } while (usedPositions.Contains(startPosition));

            int actualSectionSize = Math.Min(sectionSize, Math.Min(contentLength, extract.Length - startPosition));

            for (int i = 0; i < actualSectionSize; i++)
            {
                int currentPosition = startPosition + i;
                if (!usedPositions.Contains(currentPosition))
                {
                    contentSb.Append(extract[currentPosition]);
                    usedPositions.Add(currentPosition);
                    contentLength--;
                }
            }
        }

        return contentSb.ToString();
    }
}
