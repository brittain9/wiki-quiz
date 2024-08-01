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
}
