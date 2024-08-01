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
    public class QuestionRepositoryTests : TestBase
    {
        private readonly IQuestionRepository _repository;
        public QuestionRepositoryTests()
        {
            _repository = new QuestionRepository(_context);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsQuestion_WhenQuestionExists()
        {
            // Arrange
            await ResetDatabaseAsync();

            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TestQuestion.Id, result.Id);
            Assert.Equal(TestQuestion.Text, result.Text);
            Assert.Equal(TestQuestion.Options, result.Options);
            Assert.Equal(TestQuestion.CorrectAnswerIndex, result.CorrectAnswerIndex);
            Assert.Equal(TestQuestion.QuestionResponseId, result.QuestionResponseId);
        }
    }
}
