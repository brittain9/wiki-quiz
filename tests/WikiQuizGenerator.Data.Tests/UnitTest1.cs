using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Xunit;

using WikiQuizGenerator.Core.Models;

// Generated for my work.. Edit these
namespace WikiQuizGenerator.Tests
{
    public class QuestionRepositoryTests
    {
        [Fact]
        public void InsertAndQueryQuestions()
        {
            // Arrange
            var contextOptions = new DbContextOptionsBuilder<WikiQuizContext>()
                .UseInMemoryDatabase(databaseName: "WikiQuiz_Test")
                .Options;
        }
    }  
    
    public class WikiQuizContext : DbContext
    {
        public WikiQuizContext(DbContextOptions<WikiQuizContext> options) : base(options) 
        {
        }

        public DbSet<Question> Questions { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
    }
}