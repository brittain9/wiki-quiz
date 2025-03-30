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

        modelBuilder.Entity<User>(entity =>
        {
            // Ensure Email is unique
            entity.HasIndex(e => e.Email).IsUnique();

            // Index GoogleId for faster lookups
            entity.HasIndex(e => e.GoogleId);
        });

        modelBuilder.Entity<WikipediaPage>()
            .Property(e => e.LastModified)
            .HasColumnType(TimestampColumnType);

        modelBuilder.Entity<Quiz>()
            .Property(e => e.CreatedAt)
            .HasColumnType(TimestampColumnType);

        // Quiz - AIResponse: one-to-many relationship
        modelBuilder.Entity<Quiz>()
            .HasMany(q => q.AIResponses)
            .WithOne(r => r.Quiz)
            .HasForeignKey(r => r.QuizId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        // AIResponse - Question relationship: one-to-many relationship
        modelBuilder.Entity<AIResponse>()
            .HasMany(r => r.Questions)
            .WithOne(q => q.AIResponse)
            .HasForeignKey(q => q.AIResponseId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        // WikipediaPage - AIResponse relationship: one-to-many relationship
        modelBuilder.Entity<WikipediaPage>()
            .HasMany(p => p.AIResponses)
            .WithOne(r => r.WikipediaPage)
            .HasForeignKey(r => r.WikipediaPageId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Submission>()
            .HasOne(qs => qs.Quiz)
            .WithMany(q => q.QuizSubmissions)
            .HasForeignKey(qs => qs.QuizId)
            .OnDelete(DeleteBehavior.Restrict);

        // This is optional as entity framework would do this anyway, but for learning I will keep it
        // Create the join entity for the many-to-many relationship between page and category
        modelBuilder.Entity<WikipediaPage>()
            .HasMany(e => e.Categories)
            .WithMany(e => e.WikipediaPages)
            .UsingEntity(
                "WikipediaPageCategory",
                c => c.HasOne(typeof(WikipediaCategory)).WithMany().HasForeignKey("WikipediaCategoryId").HasPrincipalKey(nameof(WikipediaCategory.Id)),
                p => p.HasOne(typeof(WikipediaPage)).WithMany().HasForeignKey("WikipediaPageId").HasPrincipalKey(nameof(WikipediaPage.Id)),
                j => j.HasKey("WikipediaCategoryId", "WikipediaPageId"));
    }

    public DbSet<User> Users { get; set; }
    public DbSet<WikipediaPage> WikipediaPages { get; set; }
    public DbSet<WikipediaCategory> WikipediaCategories { get; set; }
    public DbSet<Submission> QuizSubmissions { get; set; }
    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<AIResponse> AIResponses { get; set; }
    public DbSet<Question> Questions { get; set; }
}
