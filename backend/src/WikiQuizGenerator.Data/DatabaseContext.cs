using Microsoft.EntityFrameworkCore;
using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Data;

public class WikiQuizDbContext : DbContext
{
    public WikiQuizDbContext(DbContextOptions<WikiQuizDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<WikipediaPageCategory>()
            .HasKey(wpc => new { wpc.WikipediaPageId, wpc.WikipediaCategoryId });

        modelBuilder.Entity<WikipediaPage>()
            .Property(e => e.LastModified)
            .HasColumnType("timestamp with time zone");

        modelBuilder.Entity<Quiz>()
            .Property(e => e.CreatedAt)
            .HasColumnType("timestamp with time zone");

    }

    public DbSet<WikipediaPage> WikipediaPages { get; set; }
    public DbSet<WikipediaCategory> WikipediaCategories { get; set; }
    public DbSet<WikipediaLink> WikipediaLinks { get; set; }
    public DbSet<WikipediaPageCategory> WikipediaPageCategories { get; set; }

    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<AIResponse> AIResponses { get; set; }
    public DbSet<AIMetadata> AIMetadata { get; set; }
    public DbSet<Question> Questions { get; set; }
}
