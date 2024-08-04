using Microsoft.EntityFrameworkCore;
using WikiQuizGenerator.Core.Models;
using WikiQuizGenerator.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WikiQuizGenerator.Data;

public abstract class TestBase : IDisposable
{
    protected WikiQuizDbContext _context;

    public WikipediaPage TestWikipediaPage { get; private set; }
    public WikipediaCategory TestWikipediaCategory { get; private set; }
    public AIResponse TestAIResponse { get; private set; }
    public Question TestQuestion { get; private set; }
    public Quiz TestQuiz { get; private set; }
    public QuizSubmission TestQuizSubmission { get; private set; }

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
        _context.WikipediaCategories.RemoveRange(_context.WikipediaCategories);
        _context.Questions.RemoveRange(_context.Questions);
        _context.AIResponses.RemoveRange(_context.AIResponses);
        _context.Quizzes.RemoveRange(_context.Quizzes);
        _context.QuizSubmissions.RemoveRange(_context.QuizSubmissions);

        await _context.SaveChangesAsync();

        await InsertTestDataAsync();
    }

    protected async Task InsertTestDataAsync()
    {
        TestWikipediaCategory = new WikipediaCategory
        {
            Name = "Test Category"
        };
        _context.WikipediaCategories.Add(TestWikipediaCategory);
        await _context.SaveChangesAsync();

        TestWikipediaPage = new WikipediaPage
        {
            Language = "en",
            Title = "Test Wikipedia Page",
            Extract = "This is a test Wikipedia page extract.",
            LastModified = DateTime.UtcNow,
            Url = "https://en.wikipedia.org/wiki/Test",
            Length = 100,
            Links = new List<string> { "Link1", "Link2" },
            Categories = new List<WikipediaCategory> { TestWikipediaCategory }
        };
        _context.WikipediaPages.Add(TestWikipediaPage);
        await _context.SaveChangesAsync();

        TestQuiz = new Quiz
        {
            Title = "Test Quiz",
            CreatedAt = DateTime.UtcNow
        };
        _context.Quizzes.Add(TestQuiz);
        await _context.SaveChangesAsync();

        TestAIResponse = new AIResponse
        {
            PromptTokenUsage = 10,
            CompletionTokenUsage = 20,
            ResponseTime = 1000,
            ModelName = "GPT-4",
            WikipediaPageId = TestWikipediaPage.Id,
            WikipediaPage = TestWikipediaPage,
            QuizId = TestQuiz.Id,
            Quiz = TestQuiz
        };
        _context.AIResponses.Add(TestAIResponse);
        await _context.SaveChangesAsync();

        TestQuestion = new Question
        {
            Text = "What is this test question?",
            Option1 = "Option A",
            Option2 = "Option B",
            Option3 = "Option C",
            Option4 = "Option D",
            CorrectOptionNumber = 1,
            AIResponseId = TestAIResponse.Id,
            AIResponse = TestAIResponse
        };
        _context.Questions.Add(TestQuestion);
        await _context.SaveChangesAsync();

        // Update TestAIResponse with the new Question
        TestAIResponse.Questions = new List<Question> { TestQuestion };
        _context.AIResponses.Update(TestAIResponse);

        // Update TestQuiz with the new AIResponse
        TestQuiz.AIResponses = new List<AIResponse> { TestAIResponse };
        _context.Quizzes.Update(TestQuiz);

        await _context.SaveChangesAsync();

        TestQuizSubmission = new QuizSubmission
        {
            QuizId = TestQuiz.Id,
            Quiz = TestQuiz,
            Answers = new List<int> { 1 },
            SubmissionTime = DateTime.UtcNow,
            Score = 3
        };
        _context.QuizSubmissions.Add(TestQuizSubmission);
        await _context.SaveChangesAsync();

        // Update TestQuiz with the new QuizSubmission
        TestQuiz.QuizSubmissions = new List<QuizSubmission> { TestQuizSubmission };
        _context.Quizzes.Update(TestQuiz);

        await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}