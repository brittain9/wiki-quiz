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

    public async Task<Quiz> GeneratorBasicQuizAsync(string topic)
    {
        topic = topic.ToTitleCase();

        WikipediaPage page = await WikipediaContent.GetWikipediaPage(topic);
        if(page == null)
        { 
            // The topic was not found.
            return null; // In web app, this will fail fast, and ill indicate in text box that the topic is invalid
        }

        Quiz quiz = new Quiz();

        quiz.Title = topic;

        var questionsResponse = await _questionGenerator.GenerateQuestionsAsync(page, 10);
        quiz.QuestionResponses.Add(questionsResponse);

        return quiz;
    }
}
