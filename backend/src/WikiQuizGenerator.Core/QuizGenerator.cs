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
    private readonly IQuestionGenerator _questionGenerator;
    private readonly ILogger<QuizGenerator> _logger;

    public QuizGenerator(IQuestionGenerator questionGenerator, ILogger<QuizGenerator> logger)
    {
        _questionGenerator = questionGenerator;
        _logger = logger;
    }

    public async Task<Quiz> GenerateBasicQuizAsync(string topic, string language, int numQuestions, int numOptions, int extractLength)
    {
        // topic = topic.Transform(To.TitleCase); // this causes errors when searching for topics..
        WikipediaPage page = await WikipediaContent.GetWikipediaPage(topic, language); // throws exception if topic not found

        if(page == null) // The topic was not found on Wikipedia
            return null; // This will fail fast, and ill indicate in text box that the topic is invalid within a second.

        Quiz quiz = new Quiz();

        quiz.Title = page.Title;

        var content = GetRandomContentSections(page, extractLength);

        var aiResponse = await _questionGenerator.GenerateQuestionsAsync(page, content, language, numQuestions, numOptions);
        quiz.AIResponses.Add(aiResponse);

        return quiz;
    }

    private static string GetRandomContentSections(WikipediaPage page, int contentLength)
    {
        // Clamp the minimum to 500 for quality variety and the max to 50000 for token usage
        contentLength = Math.Clamp(contentLength, 500, 50000);

        var extract = page.Extract;
        var length = extract.Length;


        if (contentLength >= length)
        {
            // If the user requests more than or equal to the entire length of the extract, return it all.
            return extract;
        }

        int sectionSize = contentLength > 3000 ? 1000 : 500;

        // Get the content from the wiki page more randomly to get variety
        StringBuilder contentSb = new StringBuilder();

        while (contentLength > 0)
        {
            if (length <= sectionSize)
            {
                // If there is not enough length to grab left, just append the remaining
                contentSb.Append(extract);
                break;
            }

            Random random = new Random();

            int textStartPos = random.Next(length - sectionSize);
            int actualSectionSize = Math.Min(sectionSize, length - textStartPos);

            contentSb.Append(extract.Substring(textStartPos, actualSectionSize));

            extract = extract.Remove(textStartPos, actualSectionSize);

            length -= actualSectionSize;
            contentLength -= actualSectionSize;
        }

        return contentSb.ToString();
    }
}
