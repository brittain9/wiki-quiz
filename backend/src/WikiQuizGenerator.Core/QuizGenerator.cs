using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Core;

public class QuizGenerator : IQuizGenerator
{
    private readonly IQuestionGenerator _questionGenerator;
    public QuizGenerator(IQuestionGenerator questionGenerator)
    {
        _questionGenerator = questionGenerator;
    }

    public async Task<Quiz> GeneratorBasicQuizAsync(string topic, string language, int numQuestions, int extractLength)
    {
        topic = topic.ToTitleCase(); // This probably will have problems for chinese or other non-Latin scripts

        WikipediaPage page = await WikipediaContent.GetWikipediaPage(topic, language);

        if(page == null) // The topic was not found on Wikipedia
            return null; // This will fail fast, and ill indicate in text box that the topic is invalid within a second.

        Quiz quiz = new Quiz();

        quiz.Title = topic;

        var questionsResponse = await _questionGenerator.GenerateQuestionsAsync(page, language, numQuestions, extractLength);
        quiz.QuestionResponses.Add(questionsResponse);

        return quiz;
    }
}
