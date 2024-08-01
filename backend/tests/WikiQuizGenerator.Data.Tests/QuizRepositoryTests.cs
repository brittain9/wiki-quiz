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
    public class QuizRepositoryTests : TestBase
    {
        private readonly IQuizRepository _repository;
        public QuizRepositoryTests()
        {
            _repository = new QuizRepository(_context);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsQuiz_WhenQuizExists()
        {
            // Arrange
            await ResetDatabaseAsync();

            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TestQuiz.Id, result.Id);
            Assert.Equal(TestQuiz.Title, result.Title);
        }
    }
}
