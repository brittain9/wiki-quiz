using Microsoft.EntityFrameworkCore;
using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Data;

public class WikiQuizDbContext : DbContext
{
    public WikiQuizDbContext(DbContextOptions<WikiQuizDbContext> options)
        : base(options)
    {
    }

    public DbSet<WikipediaPage> WikipediaPages { get; set; }
    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<QuestionResponse> QuestionResponses { get; set; }
    public DbSet<Question> Questions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed WikipediaPage
        modelBuilder.Entity<WikipediaPage>().HasData(
            new WikipediaPage
            {
                Id = 1,
                Langauge = "en",
                Title = "Test Wikipedia Page",
                Extract = "This is a test Wikipedia page extract.",
                LastModified = DateTime.UtcNow,
                Url = "https://en.wikipedia.org/wiki/Test",
                Length = 100,
                Links = new List<string> { "Link1", "Link2" },
                Categories = new List<string> { "Category1", "Category2" }
            }
        );

        // Seed Quiz
        modelBuilder.Entity<Quiz>().HasData(
            new Quiz
            {
                Id = 1,
                Title = "Test Quiz"
            }
        );

        // Seed QuestionResponse
        modelBuilder.Entity<QuestionResponse>().HasData(
            new QuestionResponse
            {
                Id = 1,
                PromptTokenUsage = 10,
                CompletionTokenUsage = 20,
                AIResponseTime = 1000,
                ModelName = "GPT-4",
                WikipediaPageId = 1
            }
        );

        // Seed Question
        modelBuilder.Entity<Question>().HasData(
            new Question
            {
                Id = 1,
                Text = "What is this test question?",
                Options = new List<string> { "Option A", "Option B", "Option C", "Option D" },
                CorrectAnswerIndex = 0,
                QuestionResponseId = 1
            }
        );
    }
}
