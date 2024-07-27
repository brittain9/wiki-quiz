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

    public async Task<Quiz> GeneratorBasicQuizAsync(string topic, string langauge)
    {
        topic = topic.ToTitleCase(); // Hopefully this works for multilingual support? Might need to use some more extensive libraries now

        WikipediaPage page = await WikipediaContent.GetWikipediaPage(topic, langauge);
        if(page == null)
        { 
            // The topic was not found.
            return null; // In web app, this will fail fast, and ill indicate in text box that the topic is invalid
        }

        Quiz quiz = new Quiz();

        quiz.Title = topic;

        var questionsResponse = await _questionGenerator.GenerateQuestionsAsync(page, 5, 5000, langauge);
        quiz.QuestionResponses.Add(questionsResponse);

        return quiz;
    }
}
