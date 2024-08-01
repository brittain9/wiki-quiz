using Microsoft.EntityFrameworkCore;
using WikiQuizGenerator.Core.Models;
using WikiQuizGenerator.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WikiQuizGenerator.Data;

// The in-memory database has issues with ICollection. Testing will need to be done
// using IList in the models.
public abstract class TestBase : IDisposable
{
    protected WikiQuizDbContext _context;

    public WikipediaPage TestWikipediaPage { get; private set; }
    public QuestionResponse TestQuestionResponse { get; private set; }
    public Question TestQuestion { get; private set; }
    public Quiz TestQuiz { get; private set; }

    public TestBase()
    {
        var options = new DbContextOptionsBuilder<WikiQuizDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new WikiQuizDbContext(options);
    }

    protected async Task ResetDatabaseAsync()
    {
        _context.WikipediaPages.RemoveRange(_context.WikipediaPages);
        _context.Questions.RemoveRange(_context.Questions);
        _context.QuestionResponses.RemoveRange(_context.QuestionResponses);
        _context.Quizzes.RemoveRange(_context.Quizzes);
        await _context.SaveChangesAsync();

        await InsertTestDataAsync();
    }

    protected async Task InsertTestDataAsync()
    {
        TestWikipediaPage = new WikipediaPage
        {
            Langauge = "en",
            Title = "Test Wikipedia Page",
            Extract = "This is a test Wikipedia page extract.",
            LastModified = DateTime.UtcNow,
            Url = "https://en.wikipedia.org/wiki/Test",
            Length = 100,
            Links = new List<string> { "Link1", "Link2" },
            Categories = new List<string> { "Category1", "Category2" }
        };
        _context.WikipediaPages.Add(TestWikipediaPage);
        await _context.SaveChangesAsync();

        TestQuestionResponse = new QuestionResponse
        {
            PromptTokenUsage = 10,
            CompletionTokenUsage = 20,
            AIResponseTime = 1000,
            ModelName = "GPT-4",
            WikipediaPageId = TestWikipediaPage.Id
        };
        _context.QuestionResponses.Add(TestQuestionResponse);
        await _context.SaveChangesAsync();

        TestQuestion = new Question
        {
            Text = "What is this test question?",
            Options = new List<string> { "Option A", "Option B", "Option C", "Option D" },
            CorrectAnswerIndex = 0,
            QuestionResponseId = TestQuestionResponse.Id
        };
        _context.Questions.Add(TestQuestion);
        await _context.SaveChangesAsync();

        TestQuiz = new Quiz
        {
            Title = "Test Quiz",
            QuestionResponses = new List<QuestionResponse> { TestQuestionResponse }
        };
        _context.Quizzes.Add(TestQuiz);
        await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
