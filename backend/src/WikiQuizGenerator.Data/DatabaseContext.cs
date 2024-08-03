using Microsoft.EntityFrameworkCore;
using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Data;

public class WikiQuizDbContext : DbContext
{
    private const string TimestampColumnType = "timestamp with time zone";

    public WikiQuizDbContext(DbContextOptions<WikiQuizDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<WikipediaPage>()
            .Property(e => e.LastModified)
            .HasColumnType(TimestampColumnType);

        modelBuilder.Entity<Quiz>()
            .Property(e => e.CreatedAt)
            .HasColumnType(TimestampColumnType);

        // Quiz - AIResponse relationship
        modelBuilder.Entity<Quiz>()
            .HasMany(q => q.AIResponses)
            .WithOne()
            .HasForeignKey("QuizId")
            .OnDelete(DeleteBehavior.Cascade);

        // AIResponse - Question relationship
        modelBuilder.Entity<AIResponse>()
            .HasMany(ar => ar.Questions)
            .WithOne(q => q.AIResponse)
            .HasForeignKey(q => q.AIResponseId)
            .OnDelete(DeleteBehavior.Cascade);

        // AIResponse - AIMetadata relationship
        modelBuilder.Entity<AIResponse>()
            .HasOne(ar => ar.AIMetadata)
            .WithOne()
            .HasForeignKey<AIMetadata>("AIResponseId")
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AIMetadata>()
            .HasOne(m => m.AIResponse)
            .WithOne(r => r.AIMetadata)
            .HasForeignKey<AIMetadata>(m => m.AIResponseId);

        // AIResponse - WikipediaPage relationship
        modelBuilder.Entity<AIResponse>()
            .HasOne(ar => ar.WikipediaPage)
            .WithMany()
            .HasForeignKey(ar => ar.WikipediaPageId)
            .OnDelete(DeleteBehavior.Restrict);  // Restrict deletion of WikipediaPage if AIResponses exist

        // WikipediaPage - WikipediaLink relationship
        modelBuilder.Entity<WikipediaPage>()
            .HasMany(wp => wp.Links)
            .WithOne(wl => wl.WikipediaPage)
            .HasForeignKey(wl => wl.WikipediaPageId)
            .OnDelete(DeleteBehavior.Cascade);

        // WikipediaPage - WikipediaCategory relationship (many-to-many)
        modelBuilder.Entity<WikipediaPageCategory>()
            .HasKey(wpc => new { wpc.WikipediaPageId, wpc.WikipediaCategoryId });

        modelBuilder.Entity<WikipediaPageCategory>()
            .HasOne(wpc => wpc.WikipediaPage)
            .WithMany()
            .HasForeignKey(wpc => wpc.WikipediaPageId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WikipediaPageCategory>()
            .HasOne(wpc => wpc.WikipediaCategory)
            .WithMany(wc => wc.PageCategories)
            .HasForeignKey(wpc => wpc.WikipediaCategoryId)
            .OnDelete(DeleteBehavior.Cascade);
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
