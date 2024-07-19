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

            using (var context = new WikiQuizContext(contextOptions))
            {
                // Create sample data
                var quiz = new Quiz
                {
                    Title = "Sample Quiz",
                    Questions = new List<Question>
                    {
                        new Question
                        {
                            Text = "What is the capital of France?",
                            Options = new List<string> { "Paris", "Berlin", "Rome", "Madrid" },
                            CorrectAnswerIndex = 0,
                        },
                        new Question
                        {
                            Text = "What is the largest ocean in the world?",
                            Options = new List<string> { "Pacific Ocean", "Atlantic Ocean", "Arctic Ocean", "Indian Ocean" },
                            CorrectAnswerIndex = 1,
                        },
                    }
                };

                // Insert sample data
                context.Quizzes.Add(quiz);
                context.SaveChanges();
            }

            // Assert
            using (var context = new WikiQuizContext(contextOptions))
            {
                var quizzesInDb = context.Quizzes.Include(q => q.Questions).ToList();
                
                Assert.Single(quizzesInDb); // Check if one quiz is inserted
                Assert.Equal(2, quizzesInDb[0].Questions.Count); // Check if two questions are inserted

                // Verify the data
                var firstQuestion = quizzesInDb[0].Questions.First();
                Assert.Equal("What is the capital of France?", firstQuestion.Text);
                Assert.Equal("Paris", firstQuestion.Options[firstQuestion.CorrectAnswerIndex]);
            }
        }
    }  
    
    // DbContext class
    public class WikiQuizContext : DbContext
    {
        public WikiQuizContext(DbContextOptions<WikiQuizContext> options) : base(options) 
        {
        }

        public DbSet<Question> Questions { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
    }
}