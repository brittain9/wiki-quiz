using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Models;
using WikiQuizGenerator.Data.Repositories;
using Xunit;

namespace WikiQuizGenerator.Data.Tests
{
    public class QuizRepositoryTests
    {
        private readonly WikiQuizDbContext _context;
        private readonly IQuizRepository _repository;
        public QuizRepositoryTests()
        {
            _context = TestContext.CreateInMemoryDatabase();
            _repository = new QuizRepository(_context);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsQuiz_WhenQuizExists()
        {
            // Arrange
            var expectedQuiz = new Quiz()
            {
                Id = 1,
                Title = "Test Quiz"
            };

            _context.Quizzes.Add(expectedQuiz);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedQuiz.Id, result.Id);
            Assert.Equal(expectedQuiz.Title, result.Title);
        }
    }
}
