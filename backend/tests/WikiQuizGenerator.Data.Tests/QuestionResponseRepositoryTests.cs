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
    public class QuestionResponseRepositoryTests : TestBase
    {
        private readonly IQuestionResponseRepository _repository;
        public QuestionResponseRepositoryTests()
        {
            _repository = new QuestionResponseRepository(_context);
        }


        [Fact]
        public async Task GetByIdAsync_ReturnsQuestionResponse_WhenQuestionResponseExists()
        {
            // Arrange
            await ResetDatabaseAsync();

            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TestQuestionResponse.Id, result.Id);
            Assert.Equal(TestQuestionResponse.PromptTokenUsage, result.PromptTokenUsage);
            Assert.Equal(TestQuestionResponse.CompletionTokenUsage, result.CompletionTokenUsage);
            Assert.Equal(TestQuestionResponse.AIResponseTime, result.AIResponseTime);
            Assert.Equal(TestQuestionResponse.ModelName, result.ModelName);
            Assert.Equal(TestQuestionResponse.WikipediaPageId, result.WikipediaPageId);
        }
    }
}
